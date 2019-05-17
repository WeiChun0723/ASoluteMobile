using System;
using Android.Content;
using Android.Views.InputMethods;
using ASolute_Mobile.CustomRenderer;
using Haulage.Droid.CustomRenderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(HideKeyBoardEntry), typeof(HideKeyBoardEntryRenderer))]
namespace Haulage.Droid.CustomRenderer
{
    public class HideKeyBoardEntryRenderer : EntryRenderer
    {
        HideKeyBoardEntry element;
        private Android.Text.InputTypes _inputType;

        public HideKeyBoardEntryRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null && e.OldElement == null)
            {
                ((HideKeyBoardEntry)Element).OnHideKeyboard_TriggerRenderer += MyEntryRenderer_HideKeyboard;
                //((CustomEntry)Element).OnFocused_TriggerRenderer += MyEntryRenderer_FocusControl;

                _inputType = Control.InputType;

                Element.PropertyChanged += (sender, eve) =>
                {
                    if (eve.PropertyName == HideKeyBoardEntry.CanShowVirtualKeyboardPropertyName)
                    {
                        if (!((HideKeyBoardEntry)Element).CanShowVirtualKeyboard)
                            Control.InputType = 0;
                        else
                            Control.InputType = _inputType;
                    }
                };

                if (!(Element as HideKeyBoardEntry).CanShowVirtualKeyboard)
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
            if (!((HideKeyBoardEntry)Element).CanShowVirtualKeyboard)
            {
                Control.InputType = 0;
                InputMethodManager inputMethodManager = Control.Context.GetSystemService(Context.InputMethodService) as InputMethodManager;
                inputMethodManager.HideSoftInputFromWindow(Control.WindowToken, HideSoftInputFlags.None);
            }
        }
    }
}
