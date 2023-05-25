using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;



namespace Accounting_of_goods_in_a_warehouse_in_a_hardware_store
{
    public partial class DataReplaceWindow : Window
    {
        private MainWindow.Product productCopy;

        public DataReplaceWindow(MainWindow.Product product)
        {
            InitializeComponent();
            productCopy = product;

            // Задаем исходные значения в элементы управления
            typeTextBox.Text = product.Type;
            brandTextBox.Text = product.Brand;
            nameTextBox.Text = product.Name;
            priceTextBox.Text = product.Price.ToString();
            quantityTextBox.Text = product.Quantity.ToString();
            codeTextBox.Text = product.Code;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите применить изменения?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Обновляем значения товара из элементов управления
                productCopy.Type = typeTextBox.Text;
                productCopy.Brand = brandTextBox.Text;
                productCopy.Name = nameTextBox.Text;

                if (!double.TryParse(priceTextBox.Text, out double price))
                {
                    MessageBox.Show("Пожалуйста, введите корректную цену.");
                    return;
                }

                if (!int.TryParse(quantityTextBox.Text, out int quantity))
                {
                    MessageBox.Show("Пожалуйста, введите корректное количество.");
                    return;
                }
                productCopy.Price = price;
                productCopy.Quantity = quantity;
                productCopy.Code = codeTextBox.Text;

                // Закрываем окно и возвращаем результат "true"
                DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем окно и возвращаем результат "false"
            DialogResult = false;
        }

    }
}
