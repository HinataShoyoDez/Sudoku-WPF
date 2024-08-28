using Microsoft.VisualBasic.ApplicationServices;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;


namespace SudokuMainCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Size = 9;
        private int[,] board = new int[Size, Size];
        private TextBox[,] cells = new TextBox[Size, Size];
        private Random random = new Random();
        private int attempt = 0;
        private List<int> numbers = Enumerable.Range(1, 9).ToList();
        private List<int> availableNumbers = new List<int>();
        private int[,] boardNumbers = new int[Size, Size];
        private List<Tuple<int, int>> hiddenCells = new List<Tuple<int, int>>();
        private DispatcherTimer timer;
        private int counter;

        private string GetFilePath()
        {
            
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDirectory = Path.Combine(documentsPath, "SudokuApp");
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }
            string fileName = "SudokuBoard.json";
            return Path.Combine(appDirectory, fileName);
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            CreateSudokuGrid();
            LoadBoardFromFile();
            CbDiff.SelectionChanged += CbDiff_SelectionChanged;
            this.Closing += OnWindowClosing;
            
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e) 
        {
            SaveBoardToFile();
        }

        private void CbDiff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateSudokuBoard();
            DisplayBoard();
            SaveBoardToFile();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GenerateSudokuBoard();
            DisplayBoard();
            SaveBoardToFile();
        }

        private void CreateSudokuGrid()
        {
            var sudokuGrid = new Grid
            {
                Margin = new Thickness(10)
            };

            for (int i = 0; i < Size; i++)
            {
                sudokuGrid.RowDefinitions.Add(new RowDefinition());
                sudokuGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            void InitializeCell(int row, int col)
            {
                var cell = new TextBox
                {
                    MaxLength = 1,
                    Width = 50,
                    Height = 50,
                    FontSize = 24,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    IsReadOnly = true,
                    Margin = new Thickness(1),
                    Padding = new Thickness(0),
                };

                cell.PreviewTextInput += (s, e) => e.Handled = !char.IsDigit(e.Text, 0);
                cell.TextChanged += (s, e) => UpdateCellBackground((TextBox)s);
                cell.PreviewMouseDown += (s, e) => DisplayRowColumnNumbers(Grid.GetRow((TextBox)s), Grid.GetColumn((TextBox)s));
                cell.PreviewMouseRightButtonDown += (s,e) => e.Handled = true;
                cell.ContextMenu = null;
                cell.ToolTip = new ToolTip { Content = string.Empty };
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, col);
                sudokuGrid.Children.Add(cell);
                cells[row, col] = cell;
            }

            void UpdateCellBackground(TextBox tb)
            {
                int rowIndex = Grid.GetRow(tb);
                int colIndex = Grid.GetColumn(tb);

                if (string.IsNullOrEmpty(tb.Text))
                {
                    
                    if (hiddenCells.Contains(new Tuple<int, int>(rowIndex, colIndex)))
                    {
                        tb.Background = Brushes.LightGray;
                        string missingNumbers = DisplayRowColumnNumbers(rowIndex, colIndex);
                        tb.ToolTip = new ToolTip
                        {
                            Content = $"Aranan Sayılar: {missingNumbers}"
                        };
                    }
                    else
                    {
                        tb.Background = Brushes.LightGray;
                        tb.ToolTip = null;
                    }
  
                }
                else
                {
                    if (int.TryParse(tb.Text, out int number))
                    {
                        
                        if (hiddenCells.Contains(new Tuple<int, int>(rowIndex, colIndex)))
                        {
                            if (board[rowIndex, colIndex] == number)
                            {
                                tb.Background = Brushes.LightGreen;
                                tb.IsReadOnly = true;
                                hiddenCells.Remove(new Tuple<int, int>(rowIndex, colIndex));
                            }
                            else
                            {
                                tb.Background = Brushes.LightCoral;
                                tb.IsReadOnly = false;
                               
                            }
                        }
                    }
                }

               
                CheckAndTriggerCompletion();
            }



            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    InitializeCell(row, col);
                }
            }

            void InitializeBorders()
            {
                for (int i = 0; i < Size; i += 3)
                {
                    for (int j = 0; j < Size; j += 3)
                    {
                        var border = new Border
                        {
                            BorderBrush = Brushes.Tomato,
                            BorderThickness = new Thickness(2),
                            Margin = new Thickness(-2),
                            VerticalAlignment = VerticalAlignment.Stretch,
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };
                        Grid.SetRow(border, i);
                        Grid.SetRowSpan(border, 3);
                        Grid.SetColumn(border, j);
                        Grid.SetColumnSpan(border, 3);
                        sudokuGrid.Children.Add(border);
                    }
                }
            }
            InitializeBorders();
            SudokuContainer.Content = sudokuGrid;
        }

        private void GenerateSudokuBoard()
        {
            attempt = 0;
            counter = 0;
            Array.Clear(board, 0, board.Length);
            if (FillBoard(0, 0))
            {
                HiddenCells();
                tboxResult.Text = $"Deneme Sayısı: {attempt}";
            }
        }

        private bool FillBoard(int row, int col)
        {
            if (row == Size)
                return true;

            if (col == Size)
                return FillBoard(row + 1, 0);

            if (availableNumbers.Count == 9)
            {
                for (int i = 0; i < Size; i++)
                {
                    break;
                    int number = getRandomNumber();
                    availableNumbers.Add(number);
                }
            }
            else
            {
                for (int i = 0; i < Size; i++)
                {
                    int number = getRandomNumber();
                    availableNumbers.Add(number);
                }
            }

            foreach (int num in availableNumbers.OrderBy(x => random.Next()))
            {
                if (IsSafe(row, col, num))
                {
                    board[row, col] = num;
                    attempt++;
                    if (FillBoard(row, col + 1))
                    {
                        boardNumbers[row, col] = num;
                        return true;
                    }
                    board[row, col] = 0;
                }
            }
            return false;
        }

        private bool IsSafe(int row, int col, int num)
        {
            for (int x = 0; x < Size; x++)
            {
                if (board[row, x] == num || board[x, col] == num)
                    return false;
            }

            int startRow = row - row % 3;
            int startCol = col - col % 3;
            for (int r = startRow; r < startRow + 3; r++)
            {
                for (int d = startCol; d < startCol + 3; d++)
                {
                    if (board[r, d] == num)
                        return false;
                }
            }
            return true;
        }
    
        private int getRandomNumber()
        {
            if (numbers.Count == 0)
            numbers.AddRange(Enumerable.Range(1, 9));
            int index = random.Next(numbers.Count);
            int selectedNumber = numbers[index];
            numbers.RemoveAt(index);
            return selectedNumber;
        }

        private void DisplayBoard()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    var cell = cells[row, col];
                    cell.ToolTip = null;

                    if (hiddenCells.Contains(new Tuple<int, int>(row, col)))
                    {
                        cell.Text = string.Empty;
                        cell.IsReadOnly = false;
                        cell.Background = Brushes.LightGray;
                        string missingNumbers = DisplayRowColumnNumbers(row, col);
                        cell.ToolTip = new ToolTip
                        {
                            Content = $"Aranan Sayılar: {missingNumbers}"
                        };
                    }
                    else
                    {
                        cell.Text = board[row, col] == 0 ? string.Empty : board[row, col].ToString();
                        cell.IsReadOnly = true;
                        cell.Background = Brushes.White;
                    } 
                }
            }
        }

        private void HiddenCells()
        {
            hiddenCells.Clear();
            
            int minCellsToHide = 0;
            int maxCellsToHide = 0;

            if (CbDiff.SelectedItem is ComboBoxItem selectedItem)
            {
                switch (selectedItem.Content.ToString())
                {
                    case "Kolay":
                        minCellsToHide = 10;
                        maxCellsToHide = 20;
                        break;
                    case "Orta":
                        minCellsToHide = 30;
                        maxCellsToHide = 40;
                        break;
                    case "Zor":
                        minCellsToHide = 40;
                        maxCellsToHide = 50;
                        break;
                    default:
                        minCellsToHide = 30;
                        maxCellsToHide = 40;
                        break;
                }
            }

            int cellsToHide = random.Next(minCellsToHide, maxCellsToHide);
            var allCells = Enumerable.Range(0, Size).SelectMany(row => Enumerable.Range(0, Size).Select(col => new Tuple<int, int>(row, col))).ToList();

            foreach (var cell in allCells.OrderBy(x => random.Next()).Take(cellsToHide))
            {
                if (board[cell.Item1, cell.Item2] != 0)
                {
                    hiddenCells.Add(cell);
                    cells[cell.Item1, cell.Item2].IsReadOnly = false;
                    cells[cell.Item1, cell.Item2].Background = Brushes.LightGray;
                    string missingNumbers = DisplayRowColumnNumbers(cell.Item1, cell.Item2);
                    cells[cell.Item1, cell.Item2].ToolTip = new ToolTip
                    {
                        Content = $"Aranan Sayılar: {missingNumbers}"
                    };
                }
            }
        }

        private string DisplayRowColumnNumbers(int rowIndex, int colIndex)
        {
            var allNumbers = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var rowValues = new HashSet<int>();
            for (int col = 0; col < Size; col++)
            {
                if (board[rowIndex, col] != 0 && !hiddenCells.Contains(new Tuple<int, int>(rowIndex, col)))
                {
                    rowValues.Add(board[rowIndex, col]);
                }
            }

            var colValues = new HashSet<int>();
            for (int row = 0; row < Size; row++)
            {
                if (board[row, colIndex] != 0 && !hiddenCells.Contains(new Tuple<int, int>(row, colIndex)))
                {
                    colValues.Add(board[row, colIndex]);
                }
            }

            var boxValues = new HashSet<int>();
            int boxStartRow = rowIndex / 3 * 3;
            int boxStartCol = colIndex / 3 * 3;

            for (int r = boxStartRow; r < boxStartRow + 3; r++)
            {
                for (int c = boxStartCol; c < boxStartCol + 3; c++)
                {
                    if (board[r, c] != 0 && !hiddenCells.Contains(new Tuple<int, int>(r, c)))
                    {
                        boxValues.Add(board[r, c]);
                    }
                }
            }

            var combinedValues = new HashSet<int>(rowValues);
            combinedValues.UnionWith(colValues);
            combinedValues.UnionWith(boxValues);

            var missingNumbers = allNumbers.Except(combinedValues).OrderBy(x => x).ToList();
            return string.Join(",", missingNumbers);
        }

        private bool IsBoardCompleted()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (cells[row, col].IsReadOnly == false || string.IsNullOrEmpty(cells[row, col].Text))
                        return false;
                    if (int.TryParse(cells[row, col].Text, out int value))
                    {
                        if (board[row, col] != value)
                            return false;
                    }
                }
            }
            return true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CheckAndTriggerCompletion()
        {
            bool isCompleted = IsBoardCompleted();

            if (isCompleted)
            {
                timer.Stop();
                MessageBox.Show("Tebrikler oyun bitti!", "Oyun Tamamlandı", MessageBoxButton.OK, MessageBoxImage.Information);
                counter = 0;
                tboxtimer.Text = counter.ToString();
                timer.Start();
                GenerateSudokuBoard();
                DisplayBoard();
            }
        }


        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            var confirmDialog = new ExitDialog { Owner = this };
            confirmDialog.ShowDialog();
            if (confirmDialog.IsConfirmed)  
            System.Windows.Application.Current.Shutdown();

        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s,e) =>
            {
                tboxtimer.Text = (++counter).ToString();
            };
            timer.Start();
        }

        private void SaveBoardToFile()
        {
            try
            {
                var boardState = new BoardState
                {
                    Board = Enumerable.Range(0, Size).Select(row =>
                        Enumerable.Range(0, Size).Select(col => board[row, col]).ToList()
                    ).ToList(),
                    UserValues = new string[Size, Size],
                    HiddenCells = hiddenCells,
                    Attempt = attempt,
                };

                for (int row = 0; row < Size; row++)
                {
                    for (int col = 0; col < Size; col++)
                    {
                        boardState.UserValues[row, col] = cells[row, col].Text;
                    }
                }

                var json = JsonConvert.SerializeObject(boardState, Formatting.Indented);
                File.WriteAllText(GetFilePath(), json);
                MessageBox.Show("Başarıyla kaydedildi!", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydedilirken bir hata meydana geldi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadBoardFromFile()
        {
            try
            {
                string filePath = GetFilePath();

                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    var json = File.ReadAllText(filePath);
                    var boardState = JsonConvert.DeserializeObject<BoardState>(json);

                    if (boardState == null)
                    {
                        MessageBox.Show("Yüklenebilecek bir veri bulunamadı.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        GenerateSudokuBoard();
                        DisplayBoard();
                        return;
                    }
         
                    Array.Clear(board, 0, board.Length);
                    hiddenCells.Clear();
               
                    for (int row = 0; row < Size; row++)
                    {
                        for (int col = 0; col < Size; col++)
                        {
                            board[row, col] = boardState.Board[row][col];   
                            cells[row, col].Text = boardState.UserValues[row, col];
                          
                            if (int.TryParse(boardState.UserValues[row, col], out int number))
                            {      
                                if (board[row, col] == number)
                                {           
                                    cells[row, col].IsReadOnly = true;
                                    cells[row, col].Background = Brushes.White;
                                    hiddenCells.Remove(new Tuple<int, int>(row, col));
                                }
                                else
                                {   
                                    cells[row, col].IsReadOnly = false;
                                    cells[row, col].Background = Brushes.LightCoral;
                                    if (!hiddenCells.Contains(new Tuple<int, int>(row, col)))
                                    {
                                        hiddenCells.Add(new Tuple<int, int>(row, col));
                                    }
                                }
                            }
                            else
                            { 
                                cells[row, col].IsReadOnly = false;
                                cells[row, col].Background = Brushes.LightGray;
                                if (!hiddenCells.Contains(new Tuple<int, int>(row, col)))
                                {
                                    hiddenCells.Add(new Tuple<int, int>(row, col));
                                }
                            }
                        }
                    }

                    hiddenCells = boardState.HiddenCells;
                    attempt = boardState.Attempt;
                    tboxResult.Text = $"Deneme Sayısı: {attempt}";
                    DisplayBoard();   
                    counter = 0;
                    tboxtimer.Text = counter.ToString();
                    timer.Start();
                }
                else
                {
                    GenerateSudokuBoard();
                    DisplayBoard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Board yüklenirken bir hata meydana geldi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                GenerateSudokuBoard();
                DisplayBoard();
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadBoardFromFile();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveBoardToFile();
        }
    }
}


