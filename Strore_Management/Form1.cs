using System.Net.Http;
using System.Net.Http.Json;

namespace Strore_Management
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private string ApiUrl => txtApiUrl.Text;

        public Form1()
        {
            InitializeComponent();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<Product>>(ApiUrl);
                if (products != null)
                {
                    dgvProducts.DataSource = products;
                    dgvProducts.AutoResizeColumns();
                }
                else
                {
                    dgvProducts.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async void btnGetAll_Click(object sender, EventArgs e)
        {
            await LoadProductsAsync();
        }

        private async void btnGetById_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtProductId.Text, out int id))
            {
                MessageBox.Show("Please enter a valid Product ID.");
                return;
            }

            try
            {
                var product = await _httpClient.GetFromJsonAsync<Product>($"{ApiUrl}/{id}");
                if (product != null)
                {
                    txtProductName.Text = product.ProductName;
                    txtCategory.Text = product.Category;
                    txtPrice.Text = product.Price.ToString();
                    txtQuantity.Text = product.QuantityInStock.ToString();
                    MessageBox.Show("Product found!");
                }
                else
                {
                    MessageBox.Show("Product not found.");
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("Product not found.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs(out var product))
                return;

            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiUrl, product);
                response.EnsureSuccessStatusCode();
                MessageBox.Show("Product added successfully!");
                ClearInputs();
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtProductId.Text, out int id))
            {
                MessageBox.Show("Please enter a valid Product ID.");
                return;
            }

            if (!ValidateInputs(out var product))
                return;

            product.ProductId = id;

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/{id}", product);
                response.EnsureSuccessStatusCode();
                MessageBox.Show("Product updated successfully!");
                ClearInputs();
                await LoadProductsAsync();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("Product not found.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtProductId.Text, out int id))
            {
                MessageBox.Show("Please enter a valid Product ID.");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this product?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                var response = await _httpClient.DeleteAsync($"{ApiUrl}/{id}");
                response.EnsureSuccessStatusCode();
                MessageBox.Show("Product deleted successfully!");
                ClearInputs();
                await LoadProductsAsync();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("Product not found.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private bool ValidateInputs(out Product product)
        {
            product = null;

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Please enter a Product Name.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCategory.Text))
            {
                MessageBox.Show("Please enter a Category.");
                return false;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Please enter a valid Price.");
                return false;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity))
            {
                MessageBox.Show("Please enter a valid Quantity.");
                return false;
            }

            product = new Product
            {
                ProductName = txtProductName.Text.Trim(),
                Category = txtCategory.Text.Trim(),
                Price = price,
                QuantityInStock = quantity
            };

            return true;
        }

        private void ClearInputs()
        {
            txtProductId.Clear();
            txtProductName.Clear();
            txtCategory.Clear();
            txtPrice.Clear();
            txtQuantity.Clear();
        }

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvProducts.Rows[e.RowIndex];
                txtProductId.Text = row.Cells["ProductId"].Value?.ToString();
                txtProductName.Text = row.Cells["ProductName"].Value?.ToString();
                txtCategory.Text = row.Cells["Category"].Value?.ToString();
                txtPrice.Text = row.Cells["Price"].Value?.ToString();
                txtQuantity.Text = row.Cells["QuantityInStock"].Value?.ToString();
            }
        }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
    }
}
