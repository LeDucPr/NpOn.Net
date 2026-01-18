using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using MicroServices.General.Contract.GeneralServiceContract.Domains;
using Tools.PgGenFldCode.Object;
using Timer = System.Windows.Forms.Timer;

namespace Tools.PgGenFldCode;

public partial class GenFldFrm : Form
{
    private readonly Dictionary<Control, (Label label, Timer timer)> _warnings = new();
    private readonly Color _warningColor = Color.OrangeRed;
    private readonly Color _normalColor = SystemColors.Window;
    private readonly List<ComboBoxSelectItem<EExecType>> _execTypeSelectItemList = new();
    private readonly List<ComboBoxSelectItem<EDbLanguage>> _langSelectItemList = new();
    private readonly string _storageFile = "config.txt";
    private Commit? _commit;
    private Analyz? _analyz;
    private Color _progressBarNormalColor;

    #region Form

    public GenFldFrm()
    {
        InitializeComponent();
        // if (!DesignMode) 
        LoadComboBoxItems();
        _progressBarNormalColor = GetProgressBarColor();
        if (File.Exists(_storageFile))
        {
            var lines = File.ReadAllLines(_storageFile);
            if (lines.Length > 0) databaseResourceTextBox.Text = lines[0];
            if (lines.Length > 1) hostCommitTextBox.Text = lines[1];
        }

        // string sql = @"
        //         WITH q AS (
        //             SELECT question.id AS q_id, question.question_text
        //             FROM ques_srv_question question
        //         )
        //         SELECT 
        //             survey.id AS survey_id,
        //             survey.title AS survey_title,
        //             question.question_text AS question_question_text,
        //             answer.score AS answer_score,
        //             *
        //         FROM ques_srv_survey survey
        //         JOIN ques_srv_question question ON survey.id = question.ques_srv_survey_id
        //         JOIN ques_srv_answer answer ON question.id = answer.ques_srv_question_id
        //         WHERE survey.id = @survey_id
        //     ";
        // Analize(sql, "Server=124.158.8.9;Port=5432;Database=postgres;User Id=postgres;Password=password");
    }

    private void LoadComboBoxItems()
    {
        // execType
        var execTypeKeys = EnumMode.GetAllInitEnum<EExecType>();
        execTypeComboBox.Items.Clear();
        foreach (var key in execTypeKeys)
        {
            string label = key.AsDefaultString();
            _execTypeSelectItemList.Add(new ComboBoxSelectItem<EExecType>(label, key));
            execTypeComboBox.Items.Add(label);
        }

        if (execTypeComboBox.Items.Count > 0)
        {
            execTypeComboBox.SelectedIndex = 0;
            execTypeComboBox.DataSource =
                _execTypeSelectItemList; // Set the data source for selection, preventing text input
            execTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        // Db lang
        var langKeys = EnumMode.GetAllInitEnum<EDbLanguage>().ToList();
        langKeys.Remove(EDbLanguage.Unknown);
        langComboBox.Items.Clear();
        foreach (var key in langKeys)
        {
            string label = key.AsDefaultString();
            _langSelectItemList.Add(new ComboBoxSelectItem<EDbLanguage>(label, key));
            langComboBox.Items.Add(label);
        }

        if (langComboBox.Items.Count > 0)
        {
            langComboBox.SelectedIndex = 0;
            langComboBox.DataSource = _langSelectItemList; // Set the data source for selection, preventing text input
            langComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }
    }

    private async void GenCodeButtonClick(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(databaseResourceTextBox.Text))
        {
            ShowWarning(databaseResourceTextBox, "Vui lòng nhập dữ liệu!", _warningColor, _normalColor);
            return;
        }

        ClearWarning(databaseResourceTextBox, _normalColor);


        if (string.IsNullOrEmpty(hostCommitTextBox.Text))
        {
            ShowWarning(hostCommitTextBox, "Vui lòng nhập dữ liệu!", _warningColor, _normalColor);
            return;
        }

        ClearWarning(hostCommitTextBox, _normalColor);

        File.WriteAllText(_storageFile, databaseResourceTextBox.Text + "\n" + hostCommitTextBox.Text);


        if (string.IsNullOrEmpty(tblMasterCodeTextBox.Text))
        {
            ShowWarning(tblMasterCodeTextBox, "Vui lòng nhập dữ liệu!", _warningColor, _normalColor);
            return;
        }

        ClearWarning(tblMasterCodeTextBox, _normalColor);


        if (string.IsNullOrEmpty(tblMasterSrvNameTextBox.Text))
        {
            ShowWarning(tblMasterSrvNameTextBox, "Vui lòng nhập dữ liệu!", _warningColor, _normalColor);
            return;
        }

        ClearWarning(tblMasterSrvNameTextBox, _normalColor);
        await AutoCommit();
    }


    #region Warning

    private void CreateWarningLabel(Control targetControl, string warningText)
    {
        if (_warnings.ContainsKey(targetControl)) return;

        var warningLabel = new Label
        {
            ForeColor = _warningColor,
            AutoSize = true,
            Visible = false,
            Text = warningText
        };

        // Đặt vị trí ngay dưới control
        warningLabel.Location = new Point(targetControl.Left,
            targetControl.Bottom + 5);

        // Thêm vào form thay vì parent (an toàn hơn)
        targetControl.FindForm()?.Controls.Add(warningLabel);

        var warningTimer = new Timer
        {
            Interval = 3000
        };
        warningTimer.Tick += (s, e) =>
        {
            warningLabel.Visible = false;
            warningTimer.Stop();
        };

        _warnings[targetControl] = (warningLabel, warningTimer);
    }

    public void ShowWarning(Control targetControl, string warningText, Color warningColor, Color normalColor)
    {
        if (!_warnings.ContainsKey(targetControl))
        {
            CreateWarningLabel(targetControl, warningText);
        }

        var (label, timer) = _warnings[targetControl];
        targetControl.BackColor = warningColor;
        label.Text = warningText;
        label.Visible = true;
        timer.Start();
    }

    public void ClearWarning(Control targetControl, Color normalColor)
    {
        if (!(_warnings?.ContainsKey(targetControl) ?? false))
            return;
        var (label, _) = _warnings[targetControl];
        targetControl.BackColor = normalColor;
        label.Visible = false;
    }

    #endregion Warning

    private void SetProgressBarValue(int value)
    {
        if (progressBar.InvokeRequired)
            progressBar.Invoke(new Action<int>(SetProgressBarValue), value);
        else
            progressBar.Value = value;
    }

    private Color GetProgressBarColor()
    {
        if (progressBar.InvokeRequired)
            return (Color)progressBar.Invoke(new Func<Color>(GetProgressBarColor));
        else
            return progressBar.ForeColor;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = false)]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private void SetProgressBarColor(Color color)
    {
        if (progressBar.InvokeRequired)
            progressBar.Invoke(new Action<Color>(SetProgressBarColor), color);
        else
        {
            progressBar.ForeColor = color;
            // 1 = Normal (Green), 2 = Error (Red), 3 = Paused (Yellow)
            // PBM_SETSTATE = 1040 (WM_USER + 16)
            int state = (color == _warningColor) ? 2 : 1;
            SendMessage(progressBar.Handle, 1040, (IntPtr)state, IntPtr.Zero);
        }
    }

    #endregion Form

    #region Auto Commit

    public async Task AutoCommit()
    {
        SetProgressBarColor(_progressBarNormalColor);
        _commit = new Commit(hostCommitTextBox.Text);
        _analyz = new Analyz();

        SetProgressBarValue(10);
        // check _execTypeSelectItemList có đang sử dụng EExecType.Query hay không
        if (_execTypeSelectItemList.Any(item =>
                item.ItemValue == EExecType.Query && item.Label == execTypeComboBox.Text))
        {
            if (string.IsNullOrEmpty(sqlTextBox.Text))
            {
                ShowWarning(sqlTextBox, "Vui lòng nhập dữ liệu!", _warningColor, _normalColor);
                return;
            }

            ClearWarning(sqlTextBox, _normalColor);
        }

        SetProgressBarValue(30);
        ComboBoxSelectItem<EExecType> baseTblExecType = (ComboBoxSelectItem<EExecType>)execTypeComboBox.SelectedItem!;

        var tblMaster = new TblMaster
        {
            Code = tblMasterCodeTextBox.Text,
            Description = !string.IsNullOrWhiteSpace(tblMasterDescTextBox.Text)
                ? tblMasterDescTextBox.Text
                : tblMasterCodeTextBox.Text,
            ExecFunc = baseTblExecType.ItemValue == EExecType.ExecFunc ? sqlTextBox.Text : null,
            Query = baseTblExecType.ItemValue == EExecType.Query ? sqlTextBox.Text : null,
            ExecType = baseTblExecType.ItemValue.EnumAsInt(),
            ServiceName = tblMasterSrvNameTextBox.Text,
            DbType = EDb.Postgres,
            CreatedAt = DateTime.UtcNow
        };
        SetProgressBarValue(40);
        (AnalyzeResultDomain? result, string message, bool status) =
            await _analyz.AnalyzeToDomain(sqlTextBox.Text, databaseResourceTextBox.Text, tblMaster);
        SetProgressBarValue(50);
        if (!status)
        {
            SetProgressBarColor(_warningColor);
            return;
        }

        List<string> emoji = ["🖕🏿", "🖕", "🖕🏻"];
        if (useLogCheckBox.Checked)
        {
            JsonViewerTextBox.Font = new Font("Segoe UI", 9F);
            JsonViewerTextBox.Text = SerializeResultToJson(result);
        }
        else
        {
            JsonViewerTextBox.Font = new Font("Segoe UI", 50F);
            JsonViewerTextBox.Text = emoji[new Random().Next(0, emoji.Count)];
        }

        if (outputGenCSharpFrameCheckBox.Checked)
        {
            cSharpReadModelTextBox.Font = new Font("Segoe UI", 9F);
            cSharpReadModelTextBox.Text = result?.ReadModelCode;
            domainTextBox.Font = new Font("Segoe UI", 9F);
            domainTextBox.Text = result?.DomainCode;
            tblFieldTextBox.Font = new Font("Segoe UI", 9F);
            tblFieldTextBox.Text = result?.ExecutionCode;
        }
        else
        {
            cSharpReadModelTextBox.Font = new Font("Segoe UI", 50F);
            cSharpReadModelTextBox.Text = emoji[new Random().Next(0, emoji.Count)];
            domainTextBox.Font = new Font("Segoe UI", 50F);
            domainTextBox.Text = emoji[new Random().Next(0, emoji.Count)];
            tblFieldTextBox.Font = new Font("Segoe UI", 50F);
            tblFieldTextBox.Text = emoji[new Random().Next(0, emoji.Count)];
        }

        if (!autoCommitCheckBox.Checked) // UI checkbox
        {
            SetProgressBarValue(100);
            return;
        }

        SetProgressBarValue(60);
        if (!(await _commit.ExecuteAsync<TblMaster[]?>([tblMaster], ERepositoryAction.Delete)).Status)
        {
            SetProgressBarColor(_warningColor);
            return;
        }

        SetProgressBarValue(80);
        if (result?.TblMaster == null)
        {
            SetProgressBarColor(_warningColor);
            return;
        }

        if (!(await _commit.ExecuteAsync<TblMaster[]?>([result.TblMaster], ERepositoryAction.Add)).Status)
        {
            SetProgressBarColor(_warningColor);
            return;
        }

        SetProgressBarValue(90);
        if (result.Fields is not { Count: > 0 })
        {
            SetProgressBarValue(100);
            return;
        }

        if (!(await _commit.ExecuteAsync<FldQueryMaster[]?>(result.Fields.ToArray(), ERepositoryAction.Add)).Status)
        {
            SetProgressBarColor(_warningColor);
            return;
        }

        SetProgressBarValue(100);

        if (autoCommitCheckBox.Checked)
        {
            autoCommitCheckBox.Checked = false;
            _ = FlashSuccess(autoCommitCheckBox);
        }
    }

    private string SerializeResultToJson(AnalyzeResultDomain? result)
    {
        if (result == null)
            return string.Empty;
        var options = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        return System.Text.Json.JsonSerializer.Serialize(result, options);
    }

    private async Task FlashSuccess(Control control)
    {
        var originalColor = control.BackColor;
        // Màu xanh giống ProgressBar (thường là Lime hoặc LightGreen)
        var flashColor = Color.LightGreen; 

        for (int i = 0; i < 3; i++)
        {
            control.BackColor = flashColor;
            await Task.Delay(150); // Nháy 3 lần trong khoảng 1s (150ms on + 150ms off) * 3 = 900ms
            control.BackColor = originalColor;
            await Task.Delay(150);
        }
    }

    #endregion Auto Commit
}