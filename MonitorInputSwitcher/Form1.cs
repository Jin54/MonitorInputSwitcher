using System.Runtime.InteropServices;
using System.Data;
using MonitorInputSwitcher.Core;
using MonitorInputSwitcher.Models;
using MonitorInputSwitcher.UI;

namespace MonitorInputSwitcher
{
    /// <summary>
    /// 모니터 입력 전환기 메인 폼
    /// </summary>
    public partial class MainForm : Form
    {
        #region 상수
        private const int GRID_COLS = 4;
        private const int GRID_ROWS = 4;
        private const int MAX_DEVICE_NAME_LENGTH = 70;
        #endregion

        #region 필드
        private readonly MonitorManager _monitorManager;
        private readonly DisplayLayoutManager _layoutManager;
        private readonly GlobalKeyboardHook _keyboardHook;
        private readonly OverlayManager _overlayManager;
        private readonly DataTable _monitorTable;
        private readonly DataGridViewComboBoxColumn _inputColumn;

        private readonly Size _cellSize = new((660 - 20) / GRID_COLS, (240 - 20) / GRID_ROWS);
        private readonly Color[] _colorPalette = {
            Color.Red, Color.Lime, Color.DeepSkyBlue, Color.Orange, 
            Color.Magenta, Color.Gold, Color.Cyan, Color.MediumPurple, 
            Color.Salmon, Color.Turquoise
        };
        #endregion

        #region 생성자
        public MainForm()
        {
            InitializeComponent();
            
            _monitorManager = new MonitorManager();
            _layoutManager = new DisplayLayoutManager(GRID_COLS, GRID_ROWS, _colorPalette);
            _keyboardHook = new GlobalKeyboardHook();
            _overlayManager = new OverlayManager();
            
            _monitorTable = CreateMonitorDataTable();
            _inputColumn = CreateInputColumn();
            
            SetupDataGrid();
            SetupKeyboardHook();
        }
        #endregion

        #region 초기화
        private DataTable CreateMonitorDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("번호", typeof(string));
            table.Columns.Add("장치명", typeof(string));
            table.Columns.Add("해상도", typeof(string));
            table.Columns.Add("입력", typeof(string));
            return table;
        }

        private DataGridViewComboBoxColumn CreateInputColumn()
        {
            var column = new DataGridViewComboBoxColumn
            {
                HeaderText = "입력",
                Name = "Input",
                DataPropertyName = "입력",
                FillWeight = 25,
                MinimumWidth = 100,
                FlatStyle = FlatStyle.Flat,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
            
            foreach (var input in InputType.GetAllInputs())
            {
                column.Items.Add(input);
            }
            
            return column;
        }

        private void SetupDataGrid()
        {
            dgvMonitors.Columns.Clear();
            dgvMonitors.AutoGenerateColumns = false;
            dgvMonitors.DataSource = _monitorTable;
            dgvMonitors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            // 컬럼 추가
            dgvMonitors.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "번호",
                Name = "Idx",
                DataPropertyName = "번호",
                FillWeight = 10,
                MinimumWidth = 40,
                ReadOnly = true
            });
            
            dgvMonitors.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "장치명",
                Name = "Device", 
                DataPropertyName = "장치명",
                FillWeight = 55,
                MinimumWidth = 200,
                ReadOnly = true
            });
            
            dgvMonitors.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "해상도",
                Name = "Resolution",
                DataPropertyName = "해상도",
                FillWeight = 25,
                MinimumWidth = 90,
                ReadOnly = true
            });
            
            dgvMonitors.Columns.Add(_inputColumn);
        }

        private void SetupKeyboardHook()
        {
            _keyboardHook.F12Pressed += (sender, e) =>
            {
                BeginInvoke(() =>
                {
                    var cursor = Cursor.Position;
                    Location = new Point(cursor.X - Width / 2, cursor.Y - Height / 2);
                });
            };
        }
        #endregion

        #region 이벤트 핸들러
        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadMonitors();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _keyboardHook?.Dispose();
            _overlayManager?.Dispose();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadMonitors();
        }

        private void btnShowDisplay_Click(object sender, EventArgs e)
        {
            if (_overlayManager.HasActiveOverlays)
            {
                _overlayManager.HideAllOverlays();
                btnShowOverlay.Text = "디스플레이 표시";
            }
            else
            {
                var monitors = _monitorManager.GetAllMonitors();
                foreach (var monitor in monitors)
                {
                    var color = _layoutManager.GetMonitorColor(monitor.DeviceName);
                    _overlayManager.ShowOverlay(monitor.FriendlyName, monitor.Bounds, color);
                }
                btnShowOverlay.Text = "디스플레이 숨기기";
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void dgvMonitors_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvMonitors.Columns[e.ColumnIndex] != _inputColumn)
                return;

            var monitors = _monitorManager.GetAllMonitors();
            if (e.RowIndex >= monitors.Count) return;

            var monitor = monitors[e.RowIndex];
            var selectedInput = _monitorTable.Rows[e.RowIndex]["입력"]?.ToString();
            
            if (string.IsNullOrEmpty(selectedInput) || !InputType.IsValidInput(selectedInput))
                return;

            await _monitorManager.SwitchInputAsync(monitor, selectedInput);
            
            // 현재 입력 새로고침
            var currentInput = _monitorManager.GetCurrentInput(monitor) ?? selectedInput;
            var validInput = InputType.IsValidInput(currentInput) ? currentInput : _inputColumn.Items[0]?.ToString();
            _monitorTable.Rows[e.RowIndex]["입력"] = validInput;
        }

        private void dgvMonitors_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvMonitors.CurrentCell?.OwningColumn != _inputColumn || e.Control is not ComboBox cb)
                return;

            cb.Items.Clear();
            foreach (var input in InputType.GetAllInputs())
            {
                cb.Items.Add(input);
            }
            cb.DrawMode = DrawMode.Normal;
        }

        private void panelLayout_Paint(object sender, PaintEventArgs e)
        {
            _layoutManager.PaintLayout(e.Graphics, panelLayout.Size);
        }
        #endregion

        #region 비공개 메서드
        private void LoadMonitors()
        {
            var monitors = _monitorManager.GetAllMonitors();
            _layoutManager.UpdateLayout(monitors);
            
            _monitorTable.Clear();
            
            for (int i = 0; i < monitors.Count; i++)
            {
                var monitor = monitors[i];
                var currentInput = _monitorManager.GetCurrentInput(monitor) ?? "?";
                var validInput = InputType.IsValidInput(currentInput) ? currentInput : _inputColumn.Items[0]?.ToString();
                
                var row = _monitorTable.NewRow();
                row["번호"] = (i + 1).ToString();
                row["장치명"] = monitor.FriendlyName;
                row["해상도"] = $"{monitor.Bounds.Width}x{monitor.Bounds.Height}";
                row["입력"] = validInput;
                _monitorTable.Rows.Add(row);
            }
            
            panelLayout.Invalidate();
        }
        #endregion
    }
}
