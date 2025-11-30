using System.Text;
using CommonObject;
using Enums;
using PgGenFldCode.Object;
using PgGenFldCode.Parser;
using ProjectEnums.GeneralEnums;

namespace PgGenFldCode
{
    public partial class GenFldFrm : Form
    {
        private List<ComboBoxSelectItem> _execTypeSelectItemList = new();
        private List<ComboBoxSelectItem> _langSelectItemList = new();

        public GenFldFrm()
        {
            InitializeComponent();
            // if (!DesignMode) 
            LoadComboBoxItems();
            string sql = @"
             WITH q AS (
                 SELECT question.id AS q_id, question.question_text
                 FROM ques_srv_question question
             )
             SELECT 
                 survey.id AS survey_id,
                 survey.title AS survey_title,
                 question.question_text AS question_question_text,
                 answer.score AS answer_score,
                 *
             FROM ques_srv_survey survey
             JOIN ques_srv_question question ON survey.id = question.ques_srv_survey_id
             JOIN ques_srv_answer answer ON question.id = answer.ques_srv_question_id
             WHERE survey.id = @survey_id
         ";
            Analize(sql);
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
                _langSelectItemList.Add(new ComboBoxSelectItem()
                {
                    Label = label,
                    ItemValue = key,
                });
                langComboBox.Items.Add(label);
            }

            if (langComboBox.Items.Count > 0)
                langComboBox.SelectedIndex = 0;
        }
        
        private void Analize(string execString)
        {
            if (string.IsNullOrWhiteSpace(execString))
                throw new ArgumentException("SQL string must not be null or empty", nameof(execString));
            try
            {
                // Parse SQL
                var parser = new SqlParser(execString);
                var node = parser.Parse();

                // Metadata provider
                string connStr =
                    "Server=124.158.8.9;Port=5432;Database=dbfacare_dev_2;User Id=anbit_dev;Password=minhandz;";
                var meta = new PgMetadataProvider(connStr);

                // Resolve (cách 2: phân tích tĩnh)
                var resolver = new QueryResolver(meta);
                var resolved = resolver.Resolve(node);

                // Output
                var sb = new StringBuilder();
                sb.AppendLine("Accessed tables:");
                foreach (var t in resolved.AccessedTables)
                    sb.AppendLine($"- {t}");

                sb.AppendLine();
                sb.AppendLine("Output columns:");
                foreach (var c in resolved.OutputColumns)
                {
                    string outputLine = string.Format(
                        "- {0,-25} | src: {1}.{2,-20} | type: {3}",
                        c.OutputName ?? "(expr)",
                        c.SourceAlias ?? "",
                        c.SourceColumn ?? "",
                        c.PgDataType ?? "unknown"
                    );
                    sb.AppendLine(outputLine);
                }

                sqlTextBox.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                sqlTextBox.Text = $"Error while analyzing SQL:\n{ex.Message}";
            }
        }

    }
}