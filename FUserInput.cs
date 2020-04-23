using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using FLib.FUtility;
using System.Windows.Data;
using System.Windows.Threading;

namespace FLibWpf2
{
    public class FUsertInput
    {
        TextBox txt = null;
        List <TextBox> txtList = null;
        protected List<InputFieldLine> _lineList = null;

        private FParameters _parameterList = null;
        bool waiting = false;
        bool resultOk = false;
        bool jsonFormat = false;
        object configObject = null;
        object result = null;
        string strResult = "";

        private const int BORDER_RADIUS = 10;
        private const int BORDER_MARGIN = 3;
        public const int TYPE_STRING = 1;
        public const int TYPE_JSON = 2;
        public const int TYPE_OBJECT = 3;
        public const int PADDING = 0;

        public Grid Container { get; set; }
        private Border _inputBoxBorder;
        private Grid _form = null;
        private Border _titleBoxBorder = null;
        private int formRow = 0;
        private int formCol = 0;
        private int maxCols = 2;

        public bool ResultOk
        {
            get
            {
                return resultOk;
            }
        }

        public string Result { get { return strResult; } }


        /// <summary>
        /// single text input box
        /// </summary>
        /// <param name="container"></param>
        /// <param name="title"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task showInputBox(Grid container, string title , string value)
        {
            this.Container = container;
            
            drawBorder();

            
            drawFormTitle(title);
            drawFormInput(value);
            drawFormButtons();


            _inputBoxBorder.Child = _form;
            await WaitDialog();
            
        }

        /// <summary>
        /// multiline inputBox
        /// </summary>
        /// <param name="container"></param>
        /// <param name="parametersList"></param>
        /// <param name="displayAll"></param>
        /// <returns></returns>
        public async Task showInputBox(Grid container, FParameters parametersList, bool displayAll = false)
        {
            if (parametersList == null) return;
            this._parameterList = parametersList;

            initDialog(container);
            drawBorder();

            string title = parametersList.Title;
            drawFormTitle(title);
            foreach (FParameter param in parametersList)
            {
                string value = "";
                value = param.Name;
                if ((param.UserInput) || (displayAll))
                    drawFieldInput(param.Name, param.Value + "", param.Label, param.UserInput);
            }
            
            drawFormButtons();


            _inputBoxBorder.Child = _form;
            await WaitDialog();
            int i = 0;
            foreach(TextBox txt in txtList)
            {
                _parameterList.Parameters[i++].Value = txt.Text;
            }


        }

        public FParameters getParams()
        {
            return _parameterList;
        }

        #region graph
        /************************************************ GRAPH ****************/

        protected void drawBorder()
        {

            Container.Visibility = System.Windows.Visibility.Visible;

            Grid bgGrid = new Grid();
            bgGrid.Background = Brushes.Black;
            bgGrid.Opacity = 0.5;
            Container.Children.Add(bgGrid);
            _inputBoxBorder = new Border();
            _inputBoxBorder.Background = Brushes.LightGray;
            _inputBoxBorder.BorderBrush = Brushes.Black;
            _inputBoxBorder.MinWidth = 450;
            _inputBoxBorder.BorderThickness = new System.Windows.Thickness(1);
            _inputBoxBorder.CornerRadius = new System.Windows.CornerRadius(BORDER_RADIUS);
            _inputBoxBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            _inputBoxBorder.VerticalAlignment = System.Windows.VerticalAlignment.Center;


            Container.Children.Add(_inputBoxBorder);

        }

        protected void drawFormTitle(string titleText)
        {
            _form = new Grid();
            

            _form.ColumnDefinitions.Add(new ColumnDefinition { });
            _form.ColumnDefinitions.Add(new ColumnDefinition { });
            _form.ColumnDefinitions[0].Width = new GridLength(120);
            
            //_form.ColumnDefinitions[2].Width = new GridLength(90);
            formRow = 0;
            /* JSON */
            _form.RowDefinitions.Add(new RowDefinition { });

            

            _titleBoxBorder = new Border();
            _titleBoxBorder.Background = Brushes.Gray;
            _titleBoxBorder.BorderBrush = Brushes.Black;
            _titleBoxBorder.MinWidth = 450;
            _titleBoxBorder.BorderThickness = new System.Windows.Thickness(0.25);
            _titleBoxBorder.CornerRadius = new System.Windows.CornerRadius(BORDER_RADIUS - BORDER_MARGIN, BORDER_RADIUS - BORDER_MARGIN , 0,0);
            _titleBoxBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            _titleBoxBorder.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            _titleBoxBorder.Margin = new Thickness(3);

            _form.Children.Add(_titleBoxBorder);
            Grid.SetColumn(_titleBoxBorder, 0);
            Grid.SetRow(_titleBoxBorder, formRow++);
            Grid.SetColumnSpan(_titleBoxBorder, maxCols);
            

            
            /*TITTLE*/
            Label title = new Label();
            title.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            title.HorizontalContentAlignment = HorizontalAlignment.Center;
            title.Content = titleText;
            title.Margin = new System.Windows.Thickness(3);
            title.FontSize = 12;
            title.FontWeight = FontWeights.UltraBold;
            _titleBoxBorder.Child = title;
/*
            _form.Children.Add(title);
            Grid.SetColumn(title, 0);
            Grid.SetRow(title, formRow++);
            Grid.SetColumnSpan(title, 2);
*/
        }

        protected InputFieldLine _currentLine = null;
        int _initFieldRow = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineId">idetify of line if not setted it is progressive Zero based</param>
        protected void drawFieldInit(int lineId = -1)
        {
            if (_form == null) _form = new Grid();
            if (_lineList == null)                      //init filed list
            { 
                _lineList = new List<InputFieldLine>();
                _initFieldRow = formRow;                //initial form row
            }
            _currentLine = new InputFieldLine();        //memorize all control list
            _currentLine.LineID = formRow - _initFieldRow;
            if (lineId > -1) _currentLine.LineID = lineId;
            _form.RowDefinitions.Add(new RowDefinition { });    //add form row
            formCol = 0;

        }

        protected void drawFieldLabel(string text)
        {
            /*LABEL*/
            Label label = new Label();
            label.Content = text;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.HorizontalContentAlignment = HorizontalAlignment.Left;
            label.Margin = new Thickness(BORDER_RADIUS * 2, BORDER_MARGIN, BORDER_MARGIN, BORDER_MARGIN);
            label.Padding = new Thickness(PADDING);
            _form.Children.Add(label);
            Grid.SetColumn(label, formCol);
            Grid.SetRow(label, formRow);
            incrementsColumns(label);
        }

        protected void drawFieldHelp(string text)
        {
            /*LABEL*/
            Label label = new Label();
            label.Content = text;
            label.VerticalAlignment = VerticalAlignment.Bottom;
            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.HorizontalContentAlignment = HorizontalAlignment.Left;
            label.FontSize = 7;
            //label.Margin = new Thickness(BORDER_RADIUS * 2, BORDER_MARGIN, BORDER_MARGIN, BORDER_MARGIN);
            _form.Children.Add(label);
            Grid.SetColumn(label, 0);
            Grid.SetRow(label, formRow);
            Grid.SetColumnSpan(label, maxCols);
            incrementsColumns(label);
        }

        protected void drawFieldTextBox(string text, string toolTipTxt="", bool multiline=false)
        {
            /*TEXT*/
            txt = new TextBox();
            txt.MinWidth = 100;
            txt.VerticalAlignment = VerticalAlignment.Center;
            txt.Margin = new Thickness(BORDER_MARGIN, BORDER_MARGIN, BORDER_MARGIN, BORDER_MARGIN);
            txt.Text = text;
            txt.Padding = new Thickness(PADDING);
            if (toolTipTxt != "") txt.ToolTip = toolTipTxt;
            if (multiline)
            {
                //txt.MinHeight = 150;
                txt.Height = 60;
                txt.TextWrapping = TextWrapping.Wrap;
                txt.AcceptsReturn = true;
                txt.AcceptsTab = true;
                txt.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                _form.RowDefinitions[formRow].Height = new GridLength(65);
            }
            _form.Children.Add(txt);
            Grid.SetColumn(txt, formCol);
            Grid.SetRow(txt, formRow);
            
            incrementsColumns(txt);
        }

        protected void drawFieldDropBox(object selected, FParameters options, string toolTipTxt = "")
        {
            /*TEXT*/
            ComboBox txt = new ComboBox();
            
            txt.MinWidth = 100;
            txt.VerticalAlignment = VerticalAlignment.Center;
            txt.Margin = new Thickness(BORDER_MARGIN, BORDER_MARGIN, BORDER_MARGIN, BORDER_MARGIN);
            txt.Padding = new Thickness(PADDING);
            if (toolTipTxt != "") txt.ToolTip = toolTipTxt;
            //txt.ItemsSource = options.Parameters
            txt.DisplayMemberPath = "Name";
            txt.SelectedValuePath = "Value";
            txt.ItemsSource = options.Parameters;
            txt.SelectedValue = selected;
            
            _form.Children.Add(txt);
            Grid.SetColumn(txt, formCol);
            Grid.SetRow(txt, formRow);

            incrementsColumns(txt);
        }

        protected void drawFieldCheckBox(string text, bool status=false)
        {
            /*CHECKBOX*/
            CheckBox check = new CheckBox();
            //check.MinWidth = 150;
            check.VerticalAlignment = VerticalAlignment.Center;
            check.Margin = new Thickness(BORDER_MARGIN, BORDER_MARGIN, BORDER_RADIUS * 2, BORDER_MARGIN);
            check.Content = text;
            check.Padding = new Thickness(PADDING);
            if (status) check.IsChecked = true;
            _form.Children.Add(check);
            Grid.SetColumn(check, formCol);
            Grid.SetRow(check, formRow);
            incrementsColumns(check);
        }

        protected void incrementsColumns(Control control=null)
        {
            if (control != null)
            {
                _currentLine.addControl(control);
            }
            formCol++;
            if (formCol > maxCols)
            {
                _form.ColumnDefinitions.Add(new ColumnDefinition { });
                maxCols = formCol;
                Grid.SetColumnSpan(_titleBoxBorder, maxCols);
            }
        }

        protected void drawFieldEnd()
        {
            _lineList.Add(_currentLine);
            if (_form == null) _form = new Grid();
            _form.RowDefinitions.Add(new RowDefinition { });
            formRow++;

        }

        public void drawFieldInput(string name, string value, string labelTitle, bool userInput = false)
        {
            if (_form == null) _form = new Grid();

            _form.RowDefinitions.Add(new RowDefinition { });
            /*LABEL*/
            Label label = new Label();
            label.Content = labelTitle;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.HorizontalContentAlignment = HorizontalAlignment.Left;
            label.Margin = new Thickness(BORDER_RADIUS * 2, BORDER_MARGIN, BORDER_MARGIN, BORDER_MARGIN);
            _form.Children.Add(label);
            Grid.SetColumn(label, 0);
            Grid.SetRow(label, formRow);

            /*TEXT*/
            txt = new TextBox();
            txt.MinWidth = 100;
            txt.VerticalAlignment = VerticalAlignment.Center;
            txt.Margin = new Thickness(BORDER_MARGIN, BORDER_MARGIN, BORDER_MARGIN, BORDER_MARGIN);
            txt.Text = value;
            _form.Children.Add(txt);
            Grid.SetColumn(txt, 1);
            Grid.SetRow(txt, formRow);

            /*CHECKBOX*/
            CheckBox check = new CheckBox();
            //check.MinWidth = 150;
            check.VerticalAlignment = VerticalAlignment.Center;
            check.Margin = new Thickness(BORDER_MARGIN, BORDER_MARGIN, BORDER_RADIUS * 2, BORDER_MARGIN);
            check.Content = "Display";
            if (userInput) check.IsChecked = true;
            _form.Children.Add(check);
            Grid.SetColumn(check, 2);
            Grid.SetRow(check, formRow);





            if (txtList == null) txtList = new List<TextBox>();
            txtList.Add(txt);

            formRow++;
        }


        public void drawFormInput(string value)
        {
            if (_form == null) _form = new Grid();

            _form.RowDefinitions.Add(new RowDefinition { });

            /*TEXT*/
            txt = new TextBox();
            txt.Width = 400;
            txt.MinHeight = 150;
            txt.TextWrapping = TextWrapping.Wrap;
            txt.AcceptsReturn = true;
            txt.AcceptsTab = true;
            txt.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            txt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            try
            {
                txt.Text = value;
            }
            catch (Exception ex) { }
            _form.Children.Add(txt);
            Grid.SetColumn(txt, 1);
            Grid.SetRow(txt, formRow);
            Grid.SetColumnSpan(txt, 2);
            formRow++;
        }

        protected void drawFormButtons() { 

            /*buttons ok*/
            _form.RowDefinitions.Add(new RowDefinition { });

            Button btnOk = new Button();
            btnOk.Content = "OK";
            _form.Children.Add(btnOk);
            Grid.SetColumn(btnOk, 1);
            Grid.SetColumnSpan(btnOk, maxCols);
            Grid.SetRow(btnOk, formRow);
            btnOk.Click += BtnOk_Click;
            btnOk.VerticalAlignment = VerticalAlignment.Center;
            btnOk.HorizontalAlignment = HorizontalAlignment.Right;
            btnOk.Width = 80;
            btnOk.Margin = new Thickness(BORDER_MARGIN, BORDER_MARGIN, BORDER_RADIUS * 2, BORDER_RADIUS);


            /*buttons Cancel*/
            Button btnCancel = new Button();
            btnCancel.Content = "Cancel";
            btnCancel.VerticalAlignment = VerticalAlignment.Center;
            btnCancel.HorizontalAlignment = HorizontalAlignment.Left;
            btnCancel.Width = 80;
            btnCancel.Margin = new Thickness(BORDER_RADIUS * 2, BORDER_MARGIN, BORDER_MARGIN, BORDER_RADIUS);
            _form.Children.Add(btnCancel);
            Grid.SetColumn(btnCancel, 0);
            Grid.SetRow(btnCancel, formRow);
            btnCancel.Click += BtnCancel_Click;


        }

        #endregion

        protected void initDialog(Grid container)
        {
            waiting = true;
            resultOk = false;
            this.Container = container;
            container.Children.Clear();
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(delegate { }));     //doMessages


        }

        protected void endDialog()
        {
            _inputBoxBorder.Child = _form;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(delegate { }));     //doMessages


        }

        public async Task WaitDialog()
        {

            var waitTask = Task.Run(async () =>
            {
                while (waiting) await Task.Delay(25);
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(200000)))
                throw new TimeoutException();
        }

        private  void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //MessageBox.Show("tea");
            waiting = false;
            resultOk = true;
            result = txt.Text;
            strResult = txt.Text;
            ((((sender as Button).Parent as Grid).Parent as Border).Parent as Grid).Visibility = Visibility.Collapsed;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            waiting = false;
            resultOk = false;
            result = null;
            ((((sender as Button).Parent as Grid).Parent as Border).Parent as Grid).Visibility = Visibility.Collapsed;

        }

    }

    public class InputFieldLine
    {
        public int LineID { get; set; }
        public List<Control> ControlList { get; set; }
        public void addControl(Control control)
        {
            if (ControlList == null) ControlList = new List<Control>();
            ControlList.Add(control);
        }
    }

}
