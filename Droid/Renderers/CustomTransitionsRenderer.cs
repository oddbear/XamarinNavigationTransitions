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
        private Page _topPage;
        private TaskCompletionSource<bool> _tsc;

        protected override async Task<bool> OnPushAsync(Page view, bool animated)
        {
            _topPage = view;
            var result = await base.OnPushAsync(view, false);

            var orderedNavigationStack = ((INavigationPageController)Element).StackCopy;
            var prevPage = orderedNavigationStack.Skip(1).FirstOrDefault();

            if (prevPage != null)
            {
                var containerToHide = GetContainerFromPage(prevPage);
                var containerToAdd = GetContainerFromPage(view);

                //Reverse situation back to before base.OnPushAsync:
                containerToAdd.Visibility = ViewStates.Invisible;
                containerToHide.Visibility = ViewStates.Visible;

                await Task.Yield(); //Magic! ;)

                containerToAdd.Visibility = ViewStates.Visible;

                await StartAnimation(containerToAdd, true);

                //Set status back:
                containerToHide.Visibility = ViewStates.Gone;
            }

            return result;
        }

        protected override async Task<bool> OnPopViewAsync(Page page, bool animated)
        {
            var orderedNavigationStack = ((INavigationPageController)Element).StackCopy;
            var prevPage = orderedNavigationStack.Skip(1).FirstOrDefault();
            _topPage = prevPage;

            if (prevPage != null)
            {
                var container = SetupPop(page, prevPage);
                await StartAnimation(container, false);
            }

            return await base.OnPopViewAsync(page, false);
        }

        protected override async Task<bool> OnPopToRootAsync(Page page, bool animated)
        {
            if (_topPage != null)
            {
                var container = SetupPop(_topPage, page);
                _topPage = null;
                await StartAnimation(container, false);
            }

            return await base.OnPopToRootAsync(page, false);
        }

        private AView SetupPop(Page currentPage, Page prevPage)
        {
            var containerToRemove = GetContainerFromPage(currentPage);
            var containerToShow = GetContainerFromPage(prevPage);

            containerToShow.Visibility = ViewStates.Visible;

            return containerToRemove;
        }

        private Task StartAnimation(AView container, bool isPush)
        {
            _tsc = new TaskCompletionSource<bool>();

            var cx = container.Width / 2;
            var cy = container.Height / 2;

            var radius = (float)Math.Sqrt(cx * cx + cy * cy);
            var initialRadius = isPush ? 0 : radius;
            var finalRadius = isPush ? radius : 0;

            var anim = ViewAnimationUtils.CreateCircularReveal(container, cx, cy, initialRadius, finalRadius);

            anim.AddListener(this);

            anim.Start();
            
            return _tsc.Task;
        }

        private AView GetContainerFromPage(Page page)
        {
            var renderer = Platform.GetRenderer(page);
            var container = (AView)renderer.ViewGroup.Parent;
            return container;
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
            //
        }

        public void OnAnimationStart(Animator animation)
        {
            //
        }
    }
}
