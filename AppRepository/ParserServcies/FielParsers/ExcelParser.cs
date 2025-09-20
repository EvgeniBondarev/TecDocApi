using OfficeOpenXml;
using OzonDomains.Models.MatchedRowSys;
using Servcies.ParserServcies.FielParsers.Models;
using Servcies.ReleasServcies.ReleaseManager;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Servcies.ParserServcies.FielParsers
{
    public class ExcelParser 
    {
        private ReleaseManager _releaseManager;
        private CultureInfo _culture;

        public ExcelParser(ReleaseManager releaseManager)
        {
            _releaseManager = releaseManager;
            _culture = _releaseManager.GetCultureInfo();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public List<Dictionary<string, string>> UpdateTableToStandartColumns(List<Dictionary<string, string>> table,
            Dictionary<string, string> columnMappings)
        {
            var updatedTableData = new List<Dictionary<string, string>>();
    
            // Определяем служебные столбцы, которые нужно сохранить как есть
            var preservedColumns = new HashSet<string> { "Целая часть", "Дробная часть" };

            foreach (var row in table)
            {
                var updatedRow = new Dictionary<string, string>();

                foreach (var column in row)
                {
                    // Если столбец нужно сохранить как есть
                    if (preservedColumns.Contains(column.Key))
                    {
                        updatedRow.Add(column.Key, column.Value);
                        continue;
                    }

                    // Для остальных столбцов применяем маппинг
                    var mappedKey = columnMappings.FirstOrDefault(n => n.Value == column.Key).Key;
                    if (mappedKey != null)
                    {
                        updatedRow.Add(mappedKey, column.Value);
                    }
                }

                updatedTableData.Add(updatedRow);
            }

            return updatedTableData;
        }

        public MemoryStream ConvertCsvToExcel(Stream csvStream, char delimiter)
        {
            csvStream.Position = 0; 

            var excelStream = new MemoryStream();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                using (var reader = new StreamReader(csvStream, Encoding.UTF8))
                {
                    int row = 1;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] cells = SplitCsvLine(line, delimiter);

                        for (int col = 0; col < cells.Length; col++)
                        {
                            worksheet.Cells[row, col + 1].Value = cells[col];
                        }
                        row++;
                    }
                }

                package.SaveAs(excelStream);
            }

            excelStream.Position = 0; 
            return excelStream;
        }

        private string[] SplitCsvLine(string line, char delimiter)
        {
            List<string> cells = new List<string>();

            bool inQuotes = false;
            StringBuilder currentCell = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    cells.Add(currentCell.ToString().Trim('"'));
                    currentCell.Clear();
                }
                else
                {
                    currentCell.Append(c);
                }
            }

            cells.Add(currentCell.ToString().Trim('"')); 
            return cells.ToArray();
        }


        public async Task<List<string>> GetTableHeadersAsync(Stream excelStream, int startRow = 1, int startColumn = 1)
        {
            return await Task.Run(() =>
            {
                using (var package = new ExcelPackage(excelStream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; 
                    var headers = new List<string>();
                    
                    foreach (var cell in worksheet.Cells[startRow, startColumn, startRow, worksheet.Dimension.End.Column])
                    {
                        headers.Add(cell.Text);
                    }

                    return headers;
                }
            });
        }

        public async Task<List<Dictionary<string, string>>> GetTableDataAsync(Stream excelStream, int startRow = 1, int startColumn = 1)
        {
            return await Task.Run(() =>
            {
                var tableData = new List<Dictionary<string, string>>();

                using (var package = new ExcelPackage(excelStream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var headers = new List<string>();
                    
                    foreach (var cell in worksheet.Cells[startRow, startColumn, startRow, worksheet.Dimension.End.Column])
                    {
                        headers.Add(cell.Text);
                    }
                    
                    for (int row = startRow + 1; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var rowData = new Dictionary<string, string>();
                        bool rowIsEmpty = true;

                        for (int col = startColumn; col <= worksheet.Dimension.End.Column; col++)
                        {
                            var cell = worksheet.Cells[row, col];
                            string cellText;

                            if (cell.Value is DateTime dateValue)
                            {
                                cellText = dateValue.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                cellText = cell.Text;
                            }
                            
                            if (!string.IsNullOrWhiteSpace(cellText))
                            {
                                rowIsEmpty = false;
                            }

                            if (col - startColumn < headers.Count && !string.IsNullOrWhiteSpace(headers[col - startColumn]))
                            {
                                rowData[headers[col - startColumn]] = cellText;
                            }
                        }
                        
                        if (!rowIsEmpty)
                        {
                            tableData.Add(rowData);
                        }
                    }
                }
                return tableData;
            });
        }
       
       public async Task<List<Dictionary<string, string>>> GetTableDataAsyncForDropbox(Stream excelStream, int startRow = 1, int startColumn = 1)
        {
            return await Task.Run(() =>
            {
                var tableData = new List<Dictionary<string, string>>();

                using (var package = new ExcelPackage(excelStream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var headers = new List<string>();
                    
                    // Собираем заголовки
                    foreach (var cell in worksheet.Cells[startRow, startColumn, startRow, worksheet.Dimension.End.Column])
                    {
                        headers.Add(cell.Text);
                    }
                    
                    // Добавляем новые заголовки для частей цены
                    var extendedHeaders = new List<string>(headers);
                    extendedHeaders.Add("Целая часть");
                    extendedHeaders.Add("Дробная часть");
                    
                    for (int row = startRow + 1; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var rowData = new Dictionary<string, string>();
                        bool rowIsEmpty = true;

                        for (int col = startColumn; col <= worksheet.Dimension.End.Column; col++)
                        {
                            var cell = worksheet.Cells[row, col];
                            string cellText;
                            string[] formats = { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                            CultureInfo provider = CultureInfo.InvariantCulture;

                            string rawText = cell.Text?.Trim();
                            if ((col - startColumn) < headers.Count && 
                                string.Equals(headers[col - startColumn]?.Trim(), "Цена", StringComparison.OrdinalIgnoreCase))
                            {
                                string normalizedText = rawText.Replace(" ", "").Replace("'", "");
                                string[] parts;

                                if (normalizedText.Contains(','))
                                {
                                    parts = normalizedText.Split(',');
                                }
                                else if (normalizedText.Contains('.'))
                                {
                                    parts = normalizedText.Split('.');
                                }
                                else
                                {
                                    parts = new string[] { normalizedText };
                                }

                                if (parts.Length == 2)
                                {
                                    rowData["Целая часть"] = parts[0];
                                    rowData["Дробная часть"] = parts[1];
                                    cellText = normalizedText;
                                    Console.WriteLine($"Разделено: целая='{parts[0]}', дробная='{parts[1]}'");
                                }
                                else if (parts.Length == 1)
                                {
                                    rowData["Целая часть"] = parts[0];
                                    rowData["Дробная часть"] = "0"; // или string.Empty
                                    cellText = normalizedText;
                                    Console.WriteLine($"Только целая часть: '{parts[0]}'");
                                }
                                else
                                {
                                    cellText = rawText;
                                    rowData["Целая часть"] = string.Empty;
                                    rowData["Дробная часть"] = string.Empty;
                                    Console.WriteLine($"Некорректный формат цены: '{rawText}'");
                                }
                            }
                            else if (DateTime.TryParseExact(rawText, formats, provider, DateTimeStyles.None, out DateTime dateValue))
                            {
                                cellText = dateValue.ToString("dd.MM.yyyy", new CultureInfo("ru-RU"));
                            }
                            else
                            {
                                cellText = rawText;
                            }

                            if (!string.IsNullOrWhiteSpace(cellText))
                            {
                                rowIsEmpty = false;
                            }

                            if (col - startColumn < headers.Count && !string.IsNullOrWhiteSpace(headers[col - startColumn]))
                            {
                                rowData[headers[col - startColumn]] = cellText;
                            }
                        }
                        
                        if (!rowIsEmpty)
                        {
                            tableData.Add(rowData);
                        }
                    }
                }
                return tableData;
            });
        }

        public static bool IsNumeric(object Expression)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        public async Task<List<MatchedRowModel>> MatchExcelDataAsync(Stream file1Stream,
                                                              Stream file2Stream,
                                                              List<MatchingColumn> matchingColumns,
                                                              int startRow1,
                                                              int startColumn1,
                                                              int startRow2,
                                                              int startColumn2)
        {
            var matchedRows = new List<MatchedRowModel>();
            
            var headersFile1 = await GetTableHeadersAsync(file1Stream, startRow1, startColumn1);
            file1Stream.Position = 0;  
            var dataFile1 = await GetTableDataAsync(file1Stream, startRow1, startColumn1);
            
            var headersFile2 = await GetTableHeadersAsync(file2Stream, startRow2, startColumn2);
            file2Stream.Position = 0;  
            var dataFile2 = await GetTableDataAsync(file2Stream, startRow2, startColumn2);
            
            headersFile1 = headersFile1.Distinct().ToList();
            headersFile2 = headersFile2.Distinct().ToList();
            
            var unmatchedRowsFromFile2 = new List<Dictionary<string, string>>();
            
            foreach (var rowFile1 in dataFile1)
            {
                var matchedRow = new MatchedRowModel
                {
                    File1Data = rowFile1,
                    File2Data = null 
                };

                var rowsToRemove = new List<Dictionary<string, string>>();
                foreach (var rowFile2 in dataFile2)
                {
                    bool isMatch = matchingColumns.All(match =>
                    {
                        var match1 = match.Item1.ToLower().Trim();
                        var match2 = match.Item2.ToLower().Trim();
                        if (!rowFile1.Any(e => e.Key.ToLower().Trim() == match1) || !rowFile2.Any(e => e.Key.ToLower().Trim() == match2))
                        {
                            return false;
                        }

                        var rowFile1List = rowFile1.ToList();
                        var rowFile2List = rowFile2.ToList();

                        var index1 = rowFile1List.FindIndex(e => e.Key.ToLower().Trim() == match1);
                        var index2 = rowFile2List.FindIndex(e => e.Key.ToLower().Trim() == match2);

                        if (index1 != -1 && index2 != -1)
                        {
                            var value1 = rowFile1List[index1].Value;
                            var value2 = rowFile2List[index2].Value;
                            return CompareValues(value1, value2);
                        }
                        return false;
                    });

                    if (isMatch)
                    {
                        if (matchedRow.File2Data == null)
                        {
                            matchedRow.File2Data = new List<Dictionary<string, string>>();
                        }

                        matchedRow.File2Data.Add(rowFile2);
                        rowsToRemove.Add(rowFile2);
                    }
                }
                
                matchedRows.Add(matchedRow);
                
                foreach (var row in rowsToRemove)
                {
                    dataFile2.Remove(row);
                }
            }

            foreach (var rowFile2 in dataFile2)
            {
                unmatchedRowsFromFile2.Add(rowFile2);
            }

            foreach (var unmatchedRow in unmatchedRowsFromFile2)
            {
                matchedRows.Add(new MatchedRowModel
                {
                    File1Data = null,
                    File2Data = new List<Dictionary<string, string>> { unmatchedRow } 
                });
            }

            return matchedRows;
        }

        public async Task<List<MatchedRowModel>> MatchExcelDataAsync(
    List<MatchedRowModel> matchedRows,
    Stream file1Stream,
    Stream file2Stream,
    List<MatchingColumn> matchingColumns,
    int startRow1,
    int startColumn1,
    int startRow2,
    int startColumn2)
        {
            if (file1Stream == null || file2Stream == null || matchingColumns == null || matchedRows == null)
            {
                return matchedRows ?? new List<MatchedRowModel>(); 
            }
            
            var headersFile1 = await GetTableHeadersAsync(file1Stream, startRow1, startColumn1);
            file1Stream.Position = 0; 
            var dataFile1 = await GetTableDataAsync(file1Stream, startRow1, startColumn1);

            var headersFile2 = await GetTableHeadersAsync(file2Stream, startRow2, startColumn2);
            file2Stream.Position = 0;  
            var dataFile2 = await GetTableDataAsync(file2Stream, startRow2, startColumn2);

            if (headersFile1 == null || headersFile2 == null || dataFile1 == null || dataFile2 == null)
            {
                return matchedRows;  
            }

            headersFile1 = headersFile1.Distinct().ToList();
            headersFile2 = headersFile2.Distinct().ToList();
            
            var existingRows = new HashSet<string>(
                                matchedRows.SelectMany(m => m.File2Data ?? new List<Dictionary<string, string>>())
                                           .Select(row => string.Join("|", row?.Values?.ToList() ?? new List<string>())));


            foreach (var rowFile1 in dataFile1)
            {
                if (rowFile1 == null) continue; 

                var matchedRow = new MatchedRowModel
                {
                    File1Data = rowFile1,
                    File2Data = new List<Dictionary<string, string>>() 
                };

                foreach (var rowFile2 in dataFile2)
                {
                    if (rowFile2 == null) continue;  
                    
                    bool isMatch = matchingColumns.All(match =>
                    {
                        if (!rowFile1.ContainsKey(match.Item1) || !rowFile2.ContainsKey(match.Item2))
                        {
                            return false;
                        }

                        var value1 = rowFile1[match.Item1];
                        var value2 = rowFile2[match.Item2];
                        
                        return CompareValues(value1, value2);
                    });

                    if (isMatch)
                    {
                        var rowKey = string.Join("|", rowFile2.Values);
                        if (!existingRows.Contains(rowKey))
                        {
                            matchedRow.File2Data.Add(rowFile2);
                            existingRows.Add(rowKey); 
                        }
                    }
                }
                
                if (matchedRow.File2Data.Any())
                {
                    matchedRows.Add(matchedRow);
                }
            }

            return matchedRows;
        }



        private bool CompareValues(object value1, object value2)
        {
            var culture = _culture;

            var formats = new[] { "d.M.yyyy", "yyyy-MM-dd HH:mm:ss" }; 

            if (DateTime.TryParseExact(value1?.ToString(), formats, culture, DateTimeStyles.None, out DateTime date1) &&
                DateTime.TryParseExact(value2?.ToString(), formats, culture, DateTimeStyles.None, out DateTime date2))
            {
                return date1.Date == date2.Date;  
            }
            
            if (decimal.TryParse(value1?.ToString(), NumberStyles.Any, culture, out decimal decimal1) &&
                decimal.TryParse(value2?.ToString(), NumberStyles.Any, culture, out decimal decimal2))
            {
                return Math.Floor(decimal1) == Math.Floor(decimal2); 
            }
            
            if (int.TryParse(value1?.ToString(), NumberStyles.Any, culture, out int int1) &&
                int.TryParse(value2?.ToString(), NumberStyles.Any, culture, out int int2))
            {
                return int1 == int2;
            }
            
            return string.Equals(value1?.ToString(), value2?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

    }
}
