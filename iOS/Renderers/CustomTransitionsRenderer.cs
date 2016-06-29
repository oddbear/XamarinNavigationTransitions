using System;
using System.Threading.Tasks;
using CoreAnimation;
using Foundation;
using NavigationTransitions.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(CustomTransitionsRenderer))]
namespace NavigationTransitions.iOS
{
    public class CustomTransitionsRenderer : Xamarin.Forms.Platform.iOS.NavigationRenderer
    {
        private void CreateAnimation(NSString type, NSString direction)
        {
            CATransition transition = CATransition.CreateAnimation();
            transition.Duration = 0.5;
            transition.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);
            transition.Type = type;
            transition.Subtype = direction;
            View.Layer.AddAnimation(transition, null);
        }

        public override void PushViewController(UIViewController viewController, bool animated)
        {
            base.PushViewController(viewController, animated);
        }

        //If pressed back button on toolbar:
        public override UIViewController PopViewController(bool animated)
        {
            if (animated)
                CreateAnimation(CAAnimation.TransitionMoveIn, CAAnimation.TransitionFromTop);
            
            return base.PopViewController(false); //Will then call OnPopViewAsync.
        }

        protected override Task<bool> OnPushAsync(Page page, bool animated)
        {
            if (animated)
                CreateAnimation(CAAnimation.TransitionMoveIn, CAAnimation.TransitionFromBottom);
            
            return base.OnPushAsync(page, false);
        }

        //If poped from: Navigation.PopAsync()
        protected override Task<bool> OnPopViewAsync(Page page, bool animated) //Wrong page?
        {
            if (animated)
                CreateAnimation(CAAnimation.TransitionReveal, CAAnimation.TransitionFromTop);
            
            return base.OnPopViewAsync(page, false);
        }
    }
}

