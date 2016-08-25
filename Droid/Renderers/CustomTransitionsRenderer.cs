using System;
using System.Reflection;
using System.Threading.Tasks;
using Android.Animation;
using Android.Views;
using Android.Views.Animations;
using NavigationTransitions.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

using System.Linq;

//https://developer.android.com/training/material/animations.html
[assembly: ExportRenderer(typeof(NavigationPage), typeof(CustomTransitionsRenderer))]
namespace NavigationTransitions.Droid
{
    public class CustomTransitionsRenderer : NavigationRenderer, Animator.IAnimatorListener
    {
        Page _topPage;
        TaskCompletionSource<bool> _tsc;

        protected override async Task<bool> OnPushAsync(Page view, bool animated)
        {
            _topPage = view;
            var result = await base.OnPushAsync(view, false);

            var orderedNavigationStack = ((INavigationPageController)Element).StackCopy;
            var thisPage = orderedNavigationStack.FirstOrDefault();
            var prevPage = orderedNavigationStack.Skip(1).FirstOrDefault();

            if (thisPage != null && prevPage != null)
            {
                _tsc = new TaskCompletionSource<bool>();

                var rendererToRemove = Platform.GetRenderer(prevPage);
                var containerToRemove = (AView)rendererToRemove.ViewGroup.Parent;

                var rendererToAdd = Platform.GetRenderer(thisPage);
                var containerToAdd = (AView)rendererToAdd.ViewGroup.Parent;

                //Inverse situation back to before base.OnPushAsync:
                containerToAdd.Visibility = ViewStates.Invisible;
                containerToRemove.Visibility = ViewStates.Visible;

                await Task.Yield(); //Magic! ;)

                var myView = containerToAdd;

                int cx = myView.Width / 2;
                int cy = myView.Height / 2;

                float finalRadius = (float)Math.Sqrt(cx * cx + cy * cy);

                Animator anim = ViewAnimationUtils.CreateCircularReveal(myView, cx, cy, 0, finalRadius);
                anim.AddListener(this);

                containerToAdd.Visibility = ViewStates.Visible;

                anim.Start();

                await _tsc.Task;

                //Set status back:
                containerToRemove.Visibility = ViewStates.Gone;
            }

            return result;
        }

        bool _popIsRunning = false;

        protected override async Task<bool> OnPopViewAsync(Page page, bool animated)
        {
            //_popIsRunning = true; //Trying without this.
            var orderedNavigationStack = ((INavigationPageController)Element).StackCopy;
            var thisPage = orderedNavigationStack.FirstOrDefault();
            var prevPage = orderedNavigationStack.Skip(1).FirstOrDefault();

            if (thisPage != null && prevPage != null)
            {
                await PopAnimation(thisPage, prevPage);
                _topPage = prevPage;
            }

            var result = await base.OnPopViewAsync(page, false);
            _popIsRunning = false;
            return result;
        }

        protected override async Task<bool> OnPopToRootAsync(Page page, bool animated)
        {

            if (!_popIsRunning && _topPage != null)
                await PopAnimation(_topPage, page);

            return await base.OnPopToRootAsync(page, false);
        }

        private Task PopAnimation(Page currentPage, Page prevPage)
        {
            _tsc = new TaskCompletionSource<bool>();

            var rendererToRemove = Platform.GetRenderer(currentPage);
            var containerToRemove = (AView)rendererToRemove.ViewGroup.Parent;

            var rendererToAdd = Platform.GetRenderer(prevPage);
            var containerToAdd = (AView)rendererToAdd.ViewGroup.Parent;

            containerToAdd.Visibility = ViewStates.Visible;

            var myView = containerToRemove;

            int cx = myView.Width / 2;
            int cy = myView.Height / 2;

            float initialRadius = (float)Math.Sqrt(cx * cx + cy * cy);

            Animator anim = ViewAnimationUtils.CreateCircularReveal(myView, cx, cy, initialRadius, 0);

            anim.AddListener(this);

            anim.Start();
            
            return _tsc.Task;
        }

        public void OnAnimationCancel(Animator animation)
        {
            _tsc.SetResult(false);
        }

        public void OnAnimationEnd(Animator animation)
        {
            _tsc.SetResult(true);
        }

        public void OnAnimationRepeat(Animator animation)
        {
            //throw new NotImplementedException();
        }

        public void OnAnimationStart(Animator animation)
        {
            //throw new NotImplementedException();
        }
    }
}

/*
 * Kommer dersom man trykker back og momentant på PopToRoot:
 * 
Cannot start this animator on a detached view!

  at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw () [0x0000c] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/exceptionservices/exceptionservicescommon.cs:143 
  at Java.Interop.JniEnvironment+StaticMethods.CallStaticObjectMethod (Java.Interop.JniObjectReference type, Java.Interop.JniMethodInfo method, Java.Interop.JniArgumentValue* args) [0x00082] in /Users/builder/data/lanes/3511/f7421548/source/Java.Interop/src/Java.Interop/Java.Interop/JniEnvironment.g.cs:12649 
  at Java.Interop.JniPeerMembers+JniStaticMethods.InvokeObjectMethod (System.String encodedMember, Java.Interop.JniArgumentValue* parameters) [0x0001b] in /Users/builder/data/lanes/3511/f7421548/source/Java.Interop/src/Java.Interop/Java.Interop/JniPeerMembers.JniStaticMethods.cs:97 
  at Android.Views.ViewAnimationUtils.CreateCircularReveal (Android.Views.View view, System.Int32 centerX, System.Int32 centerY, System.Single startRadius, System.Single endRadius) [0x0007f] in /Users/builder/data/lanes/3511/f7421548/source/monodroid/src/Mono.Android/platforms/android-23/src/generated/Android.Views.ViewAnimationUtils.cs:45 
  at NavigationTransitions.Droid.CustomTransitionsRenderer.PopAnimation (Xamarin.Forms.Page currentPage, Xamarin.Forms.Page prevPage) [0x0007d] in /Users/oddbear/git/XamarinNavigationTransitions/Droid/Renderers/CustomTransitionsRenderer.cs:115 
  at NavigationTransitions.Droid.CustomTransitionsRenderer+<OnPopToRootAsync>c__async2.MoveNext () [0x00074] in /Users/oddbear/git/XamarinNavigationTransitions/Droid/Renderers/CustomTransitionsRenderer.cs:91 
--- End of stack trace from previous location where exception was thrown ---
  at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw () [0x0000c] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/exceptionservices/exceptionservicescommon.cs:143 
  at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess (System.Threading.Tasks.Task task) [0x00047] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:187 
  at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification (System.Threading.Tasks.Task task) [0x0002e] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:156 
  at System.Runtime.CompilerServices.TaskAwaiter.ValidateEnd (System.Threading.Tasks.Task task) [0x0000b] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:128 
  at System.Runtime.CompilerServices.TaskAwaiter`1[TResult].GetResult () [0x00000] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:357 
  at Xamarin.Forms.NavigationPage+<PopToRootAsyncInner>d__89.MoveNext () [0x000ef] in C:\BuildAgent\work\aad494dc9bc9783\Xamarin.Forms.Core\NavigationPage.cs:334 
--- End of stack trace from previous location where exception was thrown ---
  at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw () [0x0000c] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/exceptionservices/exceptionservicescommon.cs:143 
  at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess (System.Threading.Tasks.Task task) [0x00047] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:187 
  at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification (System.Threading.Tasks.Task task) [0x0002e] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:156 
  at System.Runtime.CompilerServices.TaskAwaiter.ValidateEnd (System.Threading.Tasks.Task task) [0x0000b] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:128 
  at System.Runtime.CompilerServices.TaskAwaiter.GetResult () [0x00000] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:113 
  at Xamarin.Forms.NavigationPage+<PopToRootAsync>d__46.MoveNext () [0x0016b] in C:\BuildAgent\work\aad494dc9bc9783\Xamarin.Forms.Core\NavigationPage.cs:157 
--- End of stack trace from previous location where exception was thrown ---
  at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw () [0x0000c] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/exceptionservices/exceptionservicescommon.cs:143 
  at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess (System.Threading.Tasks.Task task) [0x00047] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:187 
  at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification (System.Threading.Tasks.Task task) [0x0002e] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:156 
  at System.Runtime.CompilerServices.TaskAwaiter.ValidateEnd (System.Threading.Tasks.Task task) [0x0000b] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:128 
  at System.Runtime.CompilerServices.TaskAwaiter.GetResult () [0x00000] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/TaskAwaiter.cs:113 
  at NavigationTransitions.SingleLockCommand+<Execute>c__async0.MoveNext () [0x0007c] in /Users/oddbear/git/XamarinNavigationTransitions/NavigationTransitions/Commands/SingleLockCommand.cs:49 
--- End of stack trace from previous location where exception was thrown ---
  at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw () [0x0000c] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/exceptionservices/exceptionservicescommon.cs:143 
  at System.Runtime.CompilerServices.AsyncMethodBuilderCore.<ThrowAsync>m__0 (System.Object state) [0x00000] in /Users/builder/data/lanes/3511/f7421548/source/mono/mcs/class/referencesource/mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs:1018 
  at Android.App.SyncContext+<Post>c__AnonStorey0.<>m__0 () [0x00000] in /Users/builder/data/lanes/3511/f7421548/source/xamarin-android/src/Mono.Android/Android.App/SyncContext.cs:18 
  at Java.Lang.Thread+RunnableImplementor.Run () [0x0000b] in /Users/builder/data/lanes/3511/f7421548/source/xamarin-android/src/Mono.Android/Java.Lang/Thread.cs:36 
  at Java.Lang.IRunnableInvoker.n_Run (System.IntPtr jnienv, System.IntPtr native__this) [0x00009] in /Users/builder/data/lanes/3511/f7421548/source/monodroid/src/Mono.Android/platforms/android-23/src/generated/Java.Lang.IRunnable.cs:81 
  at (wrapper dynamic-method) System.Object:a2b9a258-91c3-4b9a-9b97-5e1f41f232a2 (intptr,intptr)
  --- End of managed Java.Lang.IllegalStateException stack trace ---
java.lang.IllegalStateException: Cannot start this animator on a detached view!
    at android.view.RenderNode.addAnimator(RenderNode.java:812)
    at android.view.RenderNodeAnimator.setTarget(RenderNodeAnimator.java:300)
    at android.view.RenderNodeAnimator.setTarget(RenderNodeAnimator.java:282)
    at android.animation.RevealAnimator.<init>(RevealAnimator.java:37)
    at android.view.ViewAnimationUtils.createCircularReveal(ViewAnimationUtils.java:55)
    at md5b60ffeb829f638581ab2bb9b1a7f4f3f.ButtonRenderer_ButtonClickListener.n_onClick(Native Method)
    at md5b60ffeb829f638581ab2bb9b1a7f4f3f.ButtonRenderer_ButtonClickListener.onClick(ButtonRenderer_ButtonClickListener.java:30)
    at android.view.View.performClick(View.java:5204)
    at android.view.View$PerformClick.run(View.java:21155)
    at android.os.Handler.handleCallback(Handler.java:739)
    at android.os.Handler.dispatchMessage(Handler.java:95)
    at android.os.Looper.loop(Looper.java:148)
    at android.app.ActivityThread.main(ActivityThread.java:5422)
    at java.lang.reflect.Method.invoke(Native Method)
    at com.android.internal.os.ZygoteInit$MethodAndArgsCaller.run(ZygoteInit.java:726)
    at com.android.internal.os.ZygoteInit.main(ZygoteInit.java:616)

*/