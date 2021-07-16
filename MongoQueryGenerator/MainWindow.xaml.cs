using MongoQueryGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MongoQueryGenerator
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<SearchCondition> SearchConditions { get; set; }
        public List<Comparator> Comparators { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SortRadioButtonIsChecked { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SearchConditions = new ObservableCollection<SearchCondition>();
            Comparators = new List<Comparator>
            {
                new Comparator{Symbol = "==", Text = "Equal"},
                new Comparator{Symbol = ">", Text = "Greater"},
                new Comparator{Symbol = "<", Text = "Less"},
                new Comparator{Symbol = ">=", Text = "Greater or Equal"},
                new Comparator{Symbol = "<=", Text = "Less or Equal"},
                new Comparator{Symbol = "<>", Text = "Distinct"}
            };
            DataContext = this;
        }

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AddConditionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AreAddConditionsFieldValid())
            {
                MessageBox.Show("All fields must be set for condition in order to submit it!");
                return;
            }
            SearchConditions.Add(new SearchCondition
            {
                Field = addField.Text,
                Value = addExists.IsEnabled ? string.Empty : addValue.Text,
                Comparator = addExists.IsEnabled ? string.Empty : Comparators[addComparator.SelectedIndex].Symbol,
                Operator = addOperatorAnd.IsChecked.Value ? addOperatorAnd.Content.ToString() : addOperatorOr.Content.ToString(),
                Exists = addValue.IsEnabled ? null : addExists.IsChecked
            });

            addField.Clear();
            addOperatorAnd.IsChecked = null;
            addOperatorOr.IsChecked = null;
            addValue.Clear();
            addComparator.SelectedIndex = 0;
            addExists.IsChecked = null;
        }

        private bool AreAddConditionsFieldValid()
        {
            var validateOperatorCheck = addOperatorOr.IsChecked.HasValue || addOperatorAnd.IsChecked.HasValue;
            var validateField = !string.IsNullOrWhiteSpace(addField.Text);
            var validateFieldValue = !addExists.IsEnabled && !string.IsNullOrWhiteSpace(addValue.Text);
            var validateExists = !addValue.IsEnabled && addExists.IsChecked.HasValue && addExists.IsChecked.Value;

            return validateOperatorCheck && validateField && (validateFieldValue || validateExists);
        }

        private void ConditionsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            var col1 = 0.30;
            var col2 = 0.30;
            var col3 = 0.15;
            var col4 = 0.15;
            var col5 = 0.10;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
            gView.Columns[3].Width = workingWidth * col4;
            gView.Columns[4].Width = workingWidth * col5;
        }

        private void sortAsc_Checked(object sender, RoutedEventArgs e)
        {
            if ((sortAsc.IsChecked.HasValue || sortDesc.IsChecked.HasValue) && !SortRadioButtonIsChecked)
            {
                SortRadioButtonIsChecked = true;
                RaisePropertyChanged(nameof(SortRadioButtonIsChecked));
            }
        }

        private void sortDesc_Checked(object sender, RoutedEventArgs e)
        {
            if ((sortAsc.IsChecked.HasValue || sortDesc.IsChecked.HasValue) && !SortRadioButtonIsChecked)
            {
                SortRadioButtonIsChecked = true;
                RaisePropertyChanged(nameof(SortRadioButtonIsChecked));
            }
        }

        private void addValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            addExists.IsEnabled = !textbox.IsEnabled || textbox.Text.Trim().Length == 0;
        }

        private void addExists_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            addValue.IsEnabled = addComparator.IsEnabled = !checkbox.IsEnabled || (checkbox.IsChecked.HasValue && !checkbox.IsChecked.Value);
        }

        private void GenerateQueryButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(collection.Text))
            {
                MessageBox.Show("Collection must not be empty!");
                return;
            }

            if (sortAsc.IsChecked.HasValue && sortDesc.IsChecked.HasValue
                && string.IsNullOrWhiteSpace(limitResults.Text))
            {
                MessageBox.Show("Limit Results must be set if you sort the query");
                return;
            }

            int limit = 0;
            if (limitResults.Text.Length > 0 && !Int32.TryParse(limitResults.Text, out limit))
            {
                MessageBox.Show("Please insert an integer number in Limit Results field");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("db.");
            stringBuilder.Append($"{collection.Text.Trim()}.");

            //find
            var find = string.Empty;
            if (conditionsList.Items.IsEmpty)
            {
                find = "find({})";
            }
            else if (conditionsList.Items.Count == 1)
            {
                var item = conditionsList.Items[0] as SearchCondition;
                find = $"find({{{ParseConditionToQuery(item)}}})";
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                foreach (SearchCondition condition in conditionsList.Items)
                {
                    builder.Append($"${condition.Operator.ToLower()}: [{{{ParseConditionToQuery(condition)}}}],");
                }

                //remove , for last condition
                builder.Remove(builder.Length - 1, 1);
                find = $"find({{$and: [{builder.ToString()}]}})";
            }
            stringBuilder.Append(find);

            if (sortAsc.IsChecked.HasValue && sortAsc.IsChecked.Value)
            {
                stringBuilder.Append($".sort({{\"{sortField.Text}\":1}})");
            }
            else if (sortDesc.IsChecked.HasValue && sortDesc.IsChecked.Value)
            {
                stringBuilder.Append($".sort({{\"{sortField.Text}\":-1}})");
            }

            if (!string.IsNullOrWhiteSpace(limitResults.Text))
            {
                stringBuilder.Append($".limit({limit})");
            }

            stringBuilder.Append(";");
            generatedQuery.Text = stringBuilder.ToString();
        }

        private string ParseConditionToQuery(SearchCondition condition)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"\"{condition.Field}\":");

            //no value written, it will be considered the Exists field
            if (string.IsNullOrWhiteSpace(condition.Value))
            {
                stringBuilder.Append($"{{$exists: {condition.Exists.GetValueOrDefault()}}}");
            }
            else
            {
                //Check if null validation is made
                if ((condition.Comparator == Comparators[0].Symbol || condition.Comparator == Comparators[5].Symbol) &&
                    condition.Value.ToLower() == "null")
                {
                    if (condition.Comparator == Comparators[0].Symbol)
                    {
                        stringBuilder.Append($"null");
                    }
                    else
                    {
                        stringBuilder.Append($"{{$ne:null}}");
                    }
                }
                else
                {
                    if (condition.Comparator == Comparators[0].Symbol)
                    {
                        stringBuilder.Append($"\"{condition.Value}\"");
                    }
                    else if (condition.Comparator == Comparators[1].Symbol)
                    {
                        stringBuilder.Append($"{{$gt:\"{condition.Value}\"}}");
                    }
                    else if (condition.Comparator == Comparators[2].Symbol)
                    {
                        stringBuilder.Append($"{{$lt:\"{condition.Value}\"}}");
                    }
                    else if (condition.Comparator == Comparators[3].Symbol)
                    {
                        stringBuilder.Append($"{{$gte:\"{condition.Value}\"}}");
                    }
                    else if (condition.Comparator == Comparators[4].Symbol)
                    {
                        stringBuilder.Append($"{{$lte:\"{condition.Value}\"}}");
                    }
                    else if (condition.Comparator == Comparators[5].Symbol)
                    {
                        stringBuilder.Append($"{{$not:\"{condition.Value}\"}}");
                    }
                }
            }
            return stringBuilder.ToString();
        }
    }
}