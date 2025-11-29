using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CommonObject;
using Enums;
using PgGenFldCode.Object;
using ProjectEnums.GeneralEnums;

namespace PgGenFldCode
{
    public partial class GenFldFrm : Form
    {
        private List<ComboBoxSelectItem> _execTypeSelectItemList = new();

        public GenFldFrm()
        {
            InitializeComponent();
            // if (!DesignMode) 
            LoadComboBoxItems();
        }

        private void LoadComboBoxItems()
        {
            // execType
            var execTypeKeys = Enum.GetValues(typeof(EExecType));
            execTypeComboBox.Items.Clear();
            foreach (var key in execTypeKeys)
            {
                string label = key.AsDefaultString();
                _execTypeSelectItemList.Add(new ComboBoxSelectItem()
                {
                    Label = label,
                    ItemValue = key,
                });
                execTypeComboBox.Items.Add(label);
            }

            if (execTypeComboBox.Items.Count > 0)
                execTypeComboBox.SelectedIndex = 0;
            
            // Db lang
            var langKeys = Enum.GetValues(typeof(EDbLanguage)).Cast<EDbLanguage>().ToList();
            langKeys.Remove(EDbLanguage.Unknown);
            langComboBox.Items.Clear();
            foreach (var key in langKeys)
            {
                string label = key.AsDefaultString();
                _execTypeSelectItemList.Add(new ComboBoxSelectItem()
                {
                    Label = label,
                    ItemValue = key,
                });
                langComboBox.Items.Add(label);
            }

            if (langComboBox.Items.Count > 0)
                langComboBox.SelectedIndex = 0;
        }
    }
}