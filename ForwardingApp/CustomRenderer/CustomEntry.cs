using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomRenderer
{
    public class FocusArgs
    {
        public bool CouldFocusBeSet { get; set; }
    }

    public class CustomEntry : Entry
    {
        public CustomEntry() : base()
        {
            this.HeightRequest = 50;

            this.Focused += (sender, e) =>
            {
                if (!CanShowVirtualKeyboard)
                    HideKeyboard();
            };
        }

        #region set keyboard show or hide
        //public static BindableProperty OnEnterPressedCommandProperty = BindableProperty.Create<CustomEntry, ICommand>(x => x.OnEnterPressedCommand, null);

        /*public ICommand OnEnterPressedCommand
        {
            get { return (ICommand)this.GetValue(OnEnterPressedCommandProperty); }
            set { this.SetValue(OnEnterPressedCommandProperty, value); }
        }*/

        public event Action OnHideKeyboard_TriggerRenderer;

       
        public void HideKeyboard()
        {
            if (OnHideKeyboard_TriggerRenderer != null)
                OnHideKeyboard_TriggerRenderer();
        }

        public bool CanShowVirtualKeyboard
        {
            get { return (bool)GetValue(CanShowVirtualKeyboardProperty); }
            set { SetValue(CanShowVirtualKeyboardProperty, value); }
        }

       /* public event EventHandler OnEnterPressed;
        public void EnterPressed()
        {
            if (OnEnterPressedCommand != null)
                this.OnEnterPressedCommand.Execute(this.OnEnterPressedCommandParameter);

            if (OnEnterPressed != null)
                OnEnterPressed(this, EventArgs.Empty);
        }

        public event EventHandler OnFocusChanged;

        private void FocusChanged()
        {
            if (OnFocusChanged != null)
                OnFocusChanged(this, EventArgs.Empty);
        }*/

        public const string CanShowVirtualKeyboardPropertyName = "CanShowVirtualKeyboard";
        public static readonly BindableProperty CanShowVirtualKeyboardProperty =
            BindableProperty.Create(CanShowVirtualKeyboardPropertyName, typeof(bool), typeof(CustomEntry), true);


        /* public const string OnEnterPressedCommandParameterPropertyName = "OnEnterPressedCommandParameter";
         public static readonly BindableProperty OnEnterPressedCommandParameterProperty =
             BindableProperty.Create(OnEnterPressedCommandParameterPropertyName, typeof(string), typeof(CustomEntry), "");

         public string OnEnterPressedCommandParameter
         {
             get { return (string)GetValue(OnEnterPressedCommandParameterProperty); }
             set { SetValue(OnEnterPressedCommandParameterProperty, value); }
         }*/

       /* public event EventHandler<GenericEventArgs<FocusArgs>> OnFocused_TriggerRenderer;

        public new bool Focus()
        {
            var args = new GenericEventArgs<FocusArgs>(new FocusArgs { CouldFocusBeSet = false });
            OnFocused_TriggerRenderer?.Invoke(this, args);
            return args.EventData.CouldFocusBeSet;
        }*/
#endregion



        public static readonly BindableProperty ImageProperty =
           BindableProperty.Create(nameof(Image), typeof(string), typeof(CustomEntry), string.Empty);

        public static readonly BindableProperty LineColorProperty =
            BindableProperty.Create(nameof(LineColor), typeof(Xamarin.Forms.Color), typeof(CustomEntry), Color.DarkRed);

        public static readonly BindableProperty ImageHeightProperty =
            BindableProperty.Create(nameof(ImageHeight), typeof(int), typeof(CustomEntry), 40);

        public static readonly BindableProperty ImageWidthProperty =
            BindableProperty.Create(nameof(ImageWidth), typeof(int), typeof(CustomEntry), 40);

        public static readonly BindableProperty ImageAlignmentProperty =
            BindableProperty.Create(nameof(ImageAlignment), typeof(ImageAlignment), typeof(CustomEntry), ImageAlignment.Left);

        public Color LineColor
        {
            get { return (Color)GetValue(LineColorProperty); }
            set { SetValue(LineColorProperty, value); }
        }

        public int ImageWidth
        {
            get { return (int)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        public int ImageHeight
        {
            get { return (int)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public string Image
        {
            get { return (string)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public ImageAlignment ImageAlignment
        {
            get { return (ImageAlignment)GetValue(ImageAlignmentProperty); }
            set { SetValue(ImageAlignmentProperty, value); }
        }
    }

    public enum ImageAlignment
    {
        Left,
        Right
    }
}
