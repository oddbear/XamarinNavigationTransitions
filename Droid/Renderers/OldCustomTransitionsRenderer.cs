using System;
using System.Reflection;
using System.Threading.Tasks;
using Android.Views;
using Android.Views.Animations;
using NavigationTransitions.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

//[assembly: ExportRenderer(typeof(NavigationPage), typeof(OldCustomTransitionsRenderer))]
namespace NavigationTransitions.Droid
{
    public class OldCustomTransitionsRenderer : NavigationRenderer
    {
        private void SendDisappearing(Page page)
        {
            var pageType = typeof(Xamarin.Forms.Page);
            var sendDisappearingMethod = pageType.GetMethod("SendDisappearing", BindingFlags.NonPublic | BindingFlags.Instance);
            sendDisappearingMethod.Invoke(page, new object[0]);
        }

        private Page SwitchView(Page currentView)
        {
            var field = this.GetType().BaseType.GetField("_current", BindingFlags.NonPublic | BindingFlags.Instance);
            var pastView = (Page)field.GetValue(this);
            field.SetValue(this, currentView);
            return pastView;
        }

        bool _isAnimated;

        protected override async Task<bool> OnPushAsync(Page view, bool animated)
        {
            _isAnimated = animated;

            if (_isAnimated)
            {
                var pastView = SwitchView(null);

                var result = await base.OnPushAsync(view, false);

                var renderer = Platform.GetRenderer(pastView);
                if (pastView != null && renderer != null && renderer.ViewGroup.Parent != null)
                    SendDisappearing(pastView);

                return result;
            }
            else
                return await base.OnPushAsync(view, false);
        }

        public override void AddView(AView child)
        {
            base.AddView(child);
            if (_isAnimated)
            {
                child.Visibility = ViewStates.Visible;

                var animation = AnimationUtils.LoadAnimation(Context, Resource.Animation.abc_slide_in_bottom);
                animation.AnimationEnd += (sender, e) => child.Animation = null;
                child.Animation = animation;

                //Alternative logic (code):
                //child.TranslationX = Resources.DisplayMetrics.WidthPixels;
                //ViewPropertyAnimator animatior = child.Animate().TranslationX(0).SetDuration(200); //.ScaleX(1).ScaleY(1).Alpha(1)
            }
        }

        protected override Task<bool> OnPopViewAsync(Page page, bool animated)
        {
            if (animated)
            {
                var prevView = SwitchView(null);

                //TODO: No await (cancel), resource leak? will internal tsc will never stop?
                //However, this will likely never be a problem, as it's very little, happens infrequent, and the app will shutdown often (in comparison to the problem to the problem).
                base.OnPopViewAsync(page, true);

                IVisualElementRenderer rendererToRemove = Platform.GetRenderer(prevView);
                var containerToRemove = (ViewGroup)rendererToRemove.ViewGroup.Parent;

                var tsc = new TaskCompletionSource<bool>();

                var animation = AnimationUtils.LoadAnimation(Context, Resource.Animation.abc_slide_out_bottom);
                animation.AnimationEnd += (sender, e) =>
                {
                    containerToRemove.Animation = null;
                    containerToRemove.Visibility = ViewStates.Gone;
                    containerToRemove.TranslationX = 0;
                    base.RemoveView(containerToRemove);
                    tsc.SetResult(true);

                    VisualElement removedElement = rendererToRemove.Element;
                    rendererToRemove.Dispose();
                    if (removedElement != null)
                        Platform.SetRenderer(removedElement, null);
                };
                containerToRemove.StartAnimation(animation);

                ////TODO: Alternative flow.
                //containerToRemove.Animate().TranslationX(Resources.DisplayMetrics.WidthPixels).SetDuration(250).SetListener(new GenericAnimatorListener
                //{
                //    OnEnd = a => {
                //        base.RemoveView(containerToRemove);
                //        containerToRemove.TranslationX = 0;
                //        tsc.SetResult(true);

                //        VisualElement removedElement = rendererToRemove.Element;
                //        rendererToRemove.Dispose();
                //        if (removedElement != null)
                //            Platform.SetRenderer(removedElement, null);
                //    }
                //});

                return tsc.Task;
            }

            return base.OnPopViewAsync(page, animated);
        }
    }
}
