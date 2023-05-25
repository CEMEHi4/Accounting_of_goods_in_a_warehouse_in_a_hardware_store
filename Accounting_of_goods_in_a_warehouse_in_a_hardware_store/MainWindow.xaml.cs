using System.Collections.Generic;
using System.Reflection;
using System;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace Accounting_of_goods_in_a_warehouse_in_a_hardware_store
{
    public partial class MainWindow : Window
    {
        private const string databasePath = "database/products.json";
        private ObservableCollection<Product> products;
        private ObservableCollection<Product> cartItems;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCollections();
            UpdateProducts(true);
        }

        private void InitializeCollections()
        {
            products = new ObservableCollection<Product>();
            cartItems = new ObservableCollection<Product>();
        }

//------------------------------Работа с файлом---------------------------------------------------

        private void UpdateProducts(bool isLoad)
        {
            try
            {
                if (isLoad)
                {
                    if (File.Exists(databasePath))
                    {
                        string jsonData = File.ReadAllText(databasePath);
                        products.Clear();
                        JsonConvert.PopulateObject(jsonData, products);
                    }
                    else
                    {
                        products.Clear();
                    }

                    productDataGrid.ItemsSource = products;
                }
                else
                {
                    List<Product> updatedProducts = new List<Product>();

                    foreach (Product product in products)
                    {
                        Product existingProduct = updatedProducts.FirstOrDefault(p =>
                            p.Type == product.Type &&
                            p.Brand == product.Brand &&
                            p.Name == product.Name &&
                            p.Price == product.Price);

                        if (existingProduct != null)
                        {
                            existingProduct.Quantity += product.Quantity;
                        }
                        else
                        {
                            updatedProducts.Add(product);
                        }
                    }

                    updatedProducts.RemoveAll(p => p.Quantity == 0);

                    products.Clear();
                    foreach (Product updatedProduct in updatedProducts)
                    {
                        products.Add(updatedProduct);
                    }

                    string jsonData = JsonConvert.SerializeObject(products, Formatting.Indented);
                    File.WriteAllText(databasePath, jsonData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при {(isLoad ? "загрузке" : "сохранении")} данных: {ex.Message}");
                products.Clear();
            }
        }

        // true - загрузка false - сохранение 

//-------------------------------------------- Основное окно --------------------------------------

        // Обработчики событий для сортировки товаров по различным критериям
        private void SortProductsByProperty(string propertyName)
        {
            PropertyInfo property = typeof(Product).GetProperty(propertyName);
            if (property != null)
            {
                var sortedProducts = new ObservableCollection<Product>(products.OrderBy(p => property.GetValue(p)));
                productDataGrid.ItemsSource = sortedProducts;
                products = sortedProducts; // Обновляем коллекцию products
            }
        }

        // Обработчики событий для сортировки товаров по различным критериям
        private void SortByType_Click(object sender, RoutedEventArgs e)
        {
            SortProductsByProperty("Type");
        }

        private void SortByBrand_Click(object sender, RoutedEventArgs e)
        {
            SortProductsByProperty("Brand");
        }

        private void SortByName_Click(object sender, RoutedEventArgs e)
        {
            SortProductsByProperty("Name");
        }

        private void SortByPrice_Click(object sender, RoutedEventArgs e)
        {
            SortProductsByProperty("Price");
        }

        // Обработчик события для поиска товаров
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string searchText = searchTextBox.Text.Trim().ToLowerInvariant();

            if (!string.IsNullOrEmpty(searchText))
            {
                var foundProducts = new ObservableCollection<Product>(products.Where(p =>
                    (p.Type ?? "").ToLowerInvariant().Contains(searchText) ||
                    (p.Brand ?? "").ToLowerInvariant().Contains(searchText) ||
                    (p.Name ?? "").ToLowerInvariant().Contains(searchText) ||
                    (p.Code ?? "").ToLowerInvariant().Contains(searchText)));

                productDataGrid.ItemsSource = foundProducts.Count > 0 ? foundProducts : products;
            }
            else
            {
                UpdateProducts(true);
            }
        }

        // Обработчик события для сброса поиска и отображения всех товаров
        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            searchTextBox.Text = "";
            UpdateProducts(true);
        }

        // Обработчик события для добавления товара на выдачу
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (productDataGrid.SelectedItem is Product selectedProduct)
            {
                if (selectedProduct.Quantity > 0)
                {
                    selectedProduct.Quantity--;

                    var existingCartItem = cartItems.FirstOrDefault(p => p.Code == selectedProduct.Code);

                    if (existingCartItem != null)
                    {
                        existingCartItem.Quantity++;
                    }
                    else
                    {
                        cartItems.Add(new Product
                        {
                            Type = selectedProduct.Type,
                            Brand = selectedProduct.Brand,
                            Name = selectedProduct.Name,
                            Price = selectedProduct.Price,
                            Quantity = 1,
                            Code = selectedProduct.Code
                        });
                    }

                    cartDataGrid.ItemsSource = null;
                    cartDataGrid.ItemsSource = cartItems;

                    MessageBox.Show($"Товар: {selectedProduct.Type} {selectedProduct.Brand} {selectedProduct.Name} добавлен на выдачу.");
                    UpdateProducts(false);
                }
                else
                {
                    MessageBox.Show("Данного товара нет на складе.");
                }
            }
        }

        //Обработчик события для изменения данных о товаре
        private void DataReplace_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Хотите изменить данные товара в файле products.json?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Проверяем, есть ли выбранный товар
                if (productDataGrid.SelectedItem is Product selectedProduct)
                {
                    // Создаем копию выбранного товара, чтобы избежать прямого изменения данных
                    Product productCopy = new Product
                    {
                        Type = selectedProduct.Type,
                        Brand = selectedProduct.Brand,
                        Name = selectedProduct.Name,
                        Price = selectedProduct.Price,
                        Quantity = selectedProduct.Quantity,
                        Code = selectedProduct.Code
                    };

                    // Создаем новое окно DataReplaceWindow и передаем в него копию товара
                    DataReplaceWindow dataReplaceWindow = new DataReplaceWindow(productCopy);

                    // Открываем новое окно как диалоговое окно
                    if (dataReplaceWindow.ShowDialog() == true)
                    {
                        // Если пользователь нажал "Применить", обновляем данные выбранного товара
                        selectedProduct.Type = productCopy.Type;
                        selectedProduct.Brand = productCopy.Brand;
                        selectedProduct.Name = productCopy.Name;
                        selectedProduct.Price = productCopy.Price;
                        selectedProduct.Quantity = productCopy.Quantity;
                        selectedProduct.Code = productCopy.Code;

                        // Сохраняем изменения в файле products.json
                        UpdateProducts(false);

                        MessageBox.Show("Данные успешно изменены.");
                    }
                }
            }
        }

        //Обработчик события удаления товара из списка
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (productDataGrid.SelectedItem is Product selectedProduct)
            {
                MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить товар {selectedProduct.Type} {selectedProduct.Brand} {selectedProduct.Name} в кол-во: {selectedProduct.Quantity} ?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {

                    products.Remove(selectedProduct);

                    UpdateProducts(false);

                    MessageBox.Show($"Товар {selectedProduct.Type} {selectedProduct.Brand} {selectedProduct.Name} в кол-во: {selectedProduct.Quantity} успешно удален.");

                    UpdateProducts(true);
                }
            }
        }

        //---------------------------окно приемки товара-----------------------------------------------------------------------------------------------

        // Проверка введенных значений
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TypeTextBox.Text) ||
                string.IsNullOrEmpty(BrandTextBox.Text) ||
                string.IsNullOrEmpty(NameTextBox.Text) ||
                string.IsNullOrEmpty(PriceTextBox.Text) ||
                string.IsNullOrEmpty(QuantityTextBox.Text) ||
                string.IsNullOrEmpty(CodeTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            if (!double.TryParse(PriceTextBox.Text, out double price))
            {
                MessageBox.Show("Пожалуйста, введите корректную цену.");
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                MessageBox.Show("Пожалуйста, введите корректное количество.");
                return;
            }

            products.Add(new Product
            {
                Type = TypeTextBox.Text,
                Brand = BrandTextBox.Text,
                Name = NameTextBox.Text,
                Price = price,
                Quantity = quantity,
                Code = CodeTextBox.Text
            });

            TypeTextBox.Text = "";
            BrandTextBox.Text = "";
            NameTextBox.Text = "";
            PriceTextBox.Text = "";
            QuantityTextBox.Text = "";
            CodeTextBox.Text = "";

            UpdateProducts(false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UpdateProducts(false);
        }

//---------------------окно выдачи--------------------------------------------------

        // Возврат товара на склад(кнопка в контекстном меню(Вернуть Товар на склад))
        private void ReturnToWarehouse_Click(object sender, RoutedEventArgs e)
        {
            if (cartDataGrid.SelectedItem is Product selectedProduct)
            {
                if (selectedProduct.Quantity > 0)
                {
                    selectedProduct.Quantity--;

                    if (selectedProduct.Quantity == 0)
                    {
                        cartItems.Remove(selectedProduct);
                    }

                    products.Add(new Product
                    {
                        Type = selectedProduct.Type,
                        Brand = selectedProduct.Brand,
                        Name = selectedProduct.Name,
                        Price = selectedProduct.Price,
                        Quantity = 1,
                        Code = selectedProduct.Code
                    });

                    MessageBox.Show($"Товар {selectedProduct.Name} возвращен на склад.");

                    UpdateProducts(false);
                }
                else
                {
                    MessageBox.Show("Ячейка пустая.");
                }
            }
        }

        // Обработчик нажатия кнопки "Выдать товар"
        private void WriteOffButton_Click(object sender, RoutedEventArgs e)
        {
            if (cartDataGrid.Items.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("Хотите ли выдать данные поззиции со склада?", "Подтверждение выдачи", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    cartItems.Clear();
                    MessageBox.Show("Выдано");
                }
            }
            else
            {
                MessageBox.Show("На выдачу ничего нет.");
            }
        }

        public class Product
        {
            public string Type { get; set; }
            public string Brand { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }
            public int Quantity { get; set; }
            public string Code { get; set; }
        }
    }
}

