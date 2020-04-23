using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FLib;
using FLib.FUtility;

namespace FLibWpf2
{
    public class FParametersInput: FUsertInput
    {
        FParameters _parameters = null;
        FParameters _paramTemplate = null;
        Grid _container = null;
        string _name = "default";
        public string Title
        {
            get => _parameters == null ? "" :  _parameters.Title;
            set {
                if (_parameters == null) init();
                _parameters.Title = value; 
            }
        }

        public FParametersInput(Grid container, string name = "default")
        {
            _container = container;
            _name = name;

        }

        public void setParamValue(string paramName, object value)
        {
            if (_parameters == null) return;
            _parameters.setValue(paramName, value);
        }

        public string getStrParamValue(string paramName)
        {
            if (_parameters == null) return "";
            return _parameters.getStrValue(paramName);
        }

        private void init()
        {
            if (_parameters == null) _parameters = new FParameters(_name);
            if (_parameters.Title == "") _parameters.Title = _name;
        }
        
        public bool readConfig(string configName="", string fileName = "")
        {
            init();
            if (_parameters.load()) //read form configuration file
            {
                return true;
            }
            else              //read from template configuration
            {


                if (_paramTemplate != null)
                {

                    _parameters = _paramTemplate;
                    if (_name.Length > 0) _parameters.Name = _name;

                    
                }
                return true;
            }
            return false;
        }

        public void setParamTemplate(FParameters fParamsTemplate, bool assign = false)
        {
            //init();
            this._paramTemplate = fParamsTemplate;
            if (assign)
            {
                this._parameters = fParamsTemplate;
                if (_name.Length > 0) _parameters.Name = _name;
            }
        }

        /// <summary>
        /// input box to ask the configuration parameter list 
        /// based on current setted parameter
        /// </summary>
        /// <param name="save"></param>
        public async void askConfig(bool save=true)
        {
            init();
            await showParamInputBox( true);
            if ((save) && (ResultOk))
            {
                bool saved = _parameters.save();
                if (!saved) MessageBox.Show("ERROR saving configuration");
            }
        }

        public void loadAndAskConfig(FParameters fParamsTemplate, bool save = true)
        {
            setParamTemplate(fParamsTemplate, true);
            readConfig();
            askConfig(save);
        }

        public async void askParameters(EventHandler parentEvent) {
            init();
            await showParamInputBox();
            if ((parentEvent != null) && (ResultOk))
            {
                parentEvent(this._parameters, null);
            }
        }

        public async Task showParamInputBox( bool displayAll = false)
        {
            if ((_parameters == null) || (_container == null)) return;
            
            initDialog(_container);
            drawBorder();
            drawFormTitle(_parameters.Title);
            int iParam = 0;
            foreach (FParameter param in _parameters)
            {
                if ((param.UserInput) || (displayAll))
                {
                    drawFieldInit(iParam);
                    drawFieldLabel(param.Label);
                    
                    //drop box
                    if ((param.Options != null) && (param.Options.Count() > 0)) 
                    {
                        drawFieldDropBox(param.Value, param.getOptionsList(), param.Help);
                    }
                    else
                    {
                        bool multiline = false;
                        if ((param.Attributes != null) && (param.Attributes.Contains("MULTILINE"))) multiline = true;
                        drawFieldTextBox(param.Value.ToString(), param.Help, multiline);
                    }
                        if (displayAll)
                    {
                        drawFieldCheckBox("Display", param.UserInput);
                        drawFieldTextBox(param.Attributes, FParameter.ATTRIBUTE_HELP);
                    }
                    //drawFieldCheckBox("Mandatory", param.UserInput);
                    
                    drawFieldEnd();
                }
                iParam++;
            }

            drawFormButtons();
            endDialog();


            await WaitDialog();
            if (!ResultOk) return;
            foreach(InputFieldLine line in _lineList)
            {
                /*
                Console.WriteLine(line.LineID + " -> "
                    + (line.ControlList[0] as Label).Content + ":"
                    + "NewVal:" + (line.ControlList[1] as TextBox).Text
                    + " OldValue: " + _parameters.Parameters[line.LineID].Value.ToString()
                    + (line.ControlList[2] as CheckBox).IsChecked + " "

                    );
*/
                if (line.ControlList[1].GetType() == typeof(TextBox))
                    _parameters.Parameters[line.LineID].Value = (line.ControlList[1] as TextBox).Text;
                if (line.ControlList[1].GetType() == typeof(ComboBox))
                    _parameters.Parameters[line.LineID].Value = (line.ControlList[1] as ComboBox).SelectedValue;


                if (displayAll)
                {
                    _parameters.Parameters[line.LineID].UserInput = (line.ControlList[2] as CheckBox).IsChecked == true;
                    _parameters.Parameters[line.LineID].Attributes = (line.ControlList[3] as TextBox).Text ;
                }
            }
            //todo: foreach line get value

        }



        public async void  test(Grid container, string name)
        {
           
        }
    }


}
