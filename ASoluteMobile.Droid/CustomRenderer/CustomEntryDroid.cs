using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using ASolute_Mobile.CustomRenderer;
using ASoluteMobile.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryDroid))]
namespace ASoluteMobile.Droid
{
    public class CustomEntryDroid : EntryRenderer
    {
        CustomEntry element;
        private Android.Text.InputTypes _inputType;

        public CustomEntryDroid(Context context) : base(context)
        {
          
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var control = sender as CustomEntry;
            if (control == null) return;

            try
            {
               
            }
            catch (Exception ex)
            {

            }
        }
    

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                var gradientDrawable = new GradientDrawable();
                gradientDrawable.SetCornerRadius(60f);
                gradientDrawable.SetStroke(5, Android.Graphics.Color.Black);
                gradientDrawable.SetColor(Android.Graphics.Color.White);
                Control.SetBackground(gradientDrawable);

                Control.SetPadding(50, Control.PaddingTop, Control.PaddingRight, Control.PaddingBottom);

                element = (CustomEntry)this.Element;

                var editText = this.Control;
                if (!string.IsNullOrEmpty(element.Image))
                {
                    switch (element.ImageAlignment)
                    {
                        case ImageAlignment.Left:
                            editText.SetCompoundDrawablesWithIntrinsicBounds(GetDrawable(element.Image), null, null, null);
                            break;
                        case ImageAlignment.Right:
                            editText.SetCompoundDrawablesWithIntrinsicBounds(null, null, GetDrawable(element.Image), null);
                            break;
                    }
                }
                editText.CompoundDrawablePadding = 25;
                Control.Background.SetColorFilter(element.LineColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
            }

            if (Control != null && e.OldElement == null)
            {
                ((CustomEntry)Element).OnHideKeyboard_TriggerRenderer += MyEntryRenderer_HideKeyboard;
                //((CustomEntry)Element).OnFocused_TriggerRenderer += MyEntryRenderer_FocusControl;

                _inputType = Control.InputType;

                Element.PropertyChanged += (sender, eve) =>
                {
                    if (eve.PropertyName == CustomEntry.CanShowVirtualKeyboardPropertyName)
                    {
                        if (!((CustomEntry)Element).CanShowVirtualKeyboard)
                            Control.InputType = 0;
                        else
                            Control.InputType = _inputType;
                    }
                };

                if (!(Element as CustomEntry).CanShowVirtualKeyboard)
                    Control.InputType = 0;

                /*Control.EditorAction += (sender, args) =>
                {
                    if (args.ActionId == ImeAction.ImeNull && args.Event.Action == KeyEventActions.Down)
                    {
                        (Element as CustomEntry)?.EnterPressed();
                    }
                };*/
            }
        }


        void MyEntryRenderer_HideKeyboard()
        {
            HideKeyboard();
        }

       /* void MyEntryRenderer_FocusControl(object sender, GenericEventArgs<FocusArgs> args)
        {
            args.EventData.CouldFocusBeSet = Control.RequestFocus();
            if (!((CustomEntry)Element).CanShowVirtualKeyboard)
                HideKeyboard();
        }*/

        public void HideKeyboard()
        {
            Control.RequestFocus();
            if (!((CustomEntry)Element).CanShowVirtualKeyboard)
            {
                Control.InputType = 0;
                InputMethodManager inputMethodManager = Control.Context.GetSystemService(Context.InputMethodService) as InputMethodManager;
                inputMethodManager.HideSoftInputFromWindow(Control.WindowToken, HideSoftInputFlags.None);
            }
        }

        private BitmapDrawable GetDrawable(string imageEntryImage)
        {
            int resID = Resources.GetIdentifier(imageEntryImage, "drawable", this.Context.PackageName);
            var drawable = ContextCompat.GetDrawable(this.Context, resID);
            var bitmap = ((BitmapDrawable)drawable).Bitmap;

            return new BitmapDrawable(Resources, Bitmap.CreateScaledBitmap(bitmap, element.ImageWidth * 2, element.ImageHeight * 2, true));
        }


    }
}