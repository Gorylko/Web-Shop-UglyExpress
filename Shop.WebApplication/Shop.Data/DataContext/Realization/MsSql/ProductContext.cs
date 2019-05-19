﻿using Shop.Data.DataContext.Interfaces;
using Shop.Shared.Entities;
using Shop.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using SqlConst = Shop.Data.Constants.SqlQueryConstants;
using Typography = Shop.Shared.Constants.TypographyConstants;

namespace Shop.Data.DataContext.Realization.MsSql
{
    public class ProductContext : IProductContext
    {
        UserContext _userContext = new UserContext();
        CategoryContext _categoryContext = new CategoryContext();
        StateContext _stateContext = new StateContext();
        LocationContext _locationContext = new LocationContext();
        ImageContext _imageContext = new ImageContext();

        public void Archive(int productId)
        {
            using (SqlConnection connection = new SqlConnection(SqlConst.ConnectionToShopString))
            {
                connection.Open();
                var command = new SqlCommand($"UPDATE [Product]{Typography.NewLine}SET [IsArchive] = @isArchive, [UserId] = null{Typography.NewLine}WHERE Id = @productId", connection);
                command.Parameters.AddWithValue("@productId", productId);
                command.Parameters.AddWithValue("@isArchive", 1);
                command.ExecuteNonQuery();
            }
        }

        public IReadOnlyCollection<Product> GetAllByName(string searchParameter, string searchQuery)
        {
            using (var connection = new SqlConnection(SqlConst.ConnectionToShopString))
            {
                connection.Open();
                List<Product> products = new List<Product>();
                var command = new SqlCommand(SqlConst.SelectAllProductInDbString, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader[searchParameter].ToString().IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        products.Add(MapProduct(reader));
                    }
                }
                return products;
            }
        }

        public void Edit(Product editedProduct)
        {
            using (SqlConnection connection = new SqlConnection(SqlConst.ConnectionToShopString))
            {
                connection.Open();
                var command = new SqlCommand($"UPDATE [Product]{Typography.NewLine}SET [Name] = @name, [Description] = @description, [CategoryId] = @categoryId, [LastModifiedDate] = @lastModifiedDate, [LocationId] = @locationId, [Price] = @price, [StateId] = @stateId, [UserId] = @userId {Typography.NewLine}WHERE Id = {editedProduct.Id}", connection);
                command.Parameters.AddWithValue("@name", editedProduct.Name);
                command.Parameters.AddWithValue("@description", editedProduct.Description);
                command.Parameters.AddWithValue("@categoryId", editedProduct.Category.Id);
                command.Parameters.AddWithValue("@lastModifiedDate", editedProduct.LastModifiedDate);
                command.Parameters.AddWithValue("@locationId", editedProduct.Location.Id);
                command.Parameters.AddWithValue("@price", editedProduct.Price);
                command.Parameters.AddWithValue("@stateId", editedProduct.State.Id);
                command.Parameters.AddWithValue("@userId", editedProduct.Author.Id);
                command.ExecuteNonQuery();
            }
        }

        public IReadOnlyCollection<Product> GetByCategoryId(int categoryId)
        {
            List<Product> products = new List<Product>();
            using (var connection = new SqlConnection(SqlConst.ConnectionToShopString))
            {
                connection.Open();
                using (var command = new SqlCommand(SqlConst.SelectAllProductInDbString + $"WHERE [CategoryId] = {categoryId}", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(MapProduct(reader));
                        }
                    }
                }

                return products;
            }
        }

        public IReadOnlyCollection<Product> GetByUserId(int userId)
        {
            List<Product> products = new List<Product>();
            using (var connection = new SqlConnection(SqlConst.ConnectionToShopString))
            {
                connection.Open();
                using (var command = new SqlCommand(SqlConst.SelectAllProductInDbString + Typography.NewLine + $"WHERE [UserId] = {userId}", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(MapProduct(reader));
                        }
                    }
                }

                return products;
            }
        }

        public Product MapProduct(SqlDataReader reader)
        {
            return new Product
            {
                Id = (int)reader["Id"],
                Name = (string)reader["Name"],
                Description = (string)reader["Description"],
                Price = (decimal)reader["Price"],
                CreationDate = (DateTime)reader["CreationDate"],
                LastModifiedDate = (DateTime)reader["LastModifiedDate"],
                Category = new Category
                {
                    Id = (int)reader["CategoryId"],
                    Name = (string)reader["Category"]
                },
                Author = new User
                {
                    Id = (int)reader["UserId"],
                    Login = (string)reader["Login"],
                    Email = (string)reader["Email"],
                    PhoneNumber = (string)reader["PhoneNumber"],
                    Role = RoleHelper.ConvertToRoleType((int)reader["RoleId"])
                },
                Location = _locationContext.GetById((int)reader["LocationId"]),
                State = new State
                {
                    Id = (int)reader["StateId"],
                    Name = (string)reader["State"]
                },
                Images = _imageContext.GetAllByProductId((int)reader["Id"])
            };
        }

        public Product GetById(int id)
        {
            using (var connection = new SqlConnection(SqlConst.ConnectionToShopString))
            {
                connection.Open();
                List<Product> products = new List<Product>();
                string query = SqlConst.SelectAllProductInDbString + Typography.NewLine + $"WHERE [Product].[Id] = @id";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = command.ExecuteReader();

                reader.Read();
                return MapProduct(reader);
            }
        }

        public IReadOnlyCollection<Product> GetAll()
        {
            List<Product> returnProducts = new List<Product>();
            using (var connection = new SqlConnection(SqlConst.ConnectionToShopString))
            {
                connection.Open();
                List<Product> products = new List<Product>();
                var command = new SqlCommand(SqlConst.SelectAllProductInDbString + Typography.NewLine + "WHERE [IsArchive] = 0", connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    returnProducts.Add(MapProduct(reader));
                }
                return returnProducts;
            }
        }

        public void DeleteById(int id)
        {
            using (var connection = new SqlConnection(SqlConst.ConnectionToShopString))
            {
                connection.Open();
                var command = new SqlCommand($"DELETE [Product] WHERE [Id] = {id}", connection);
                command.ExecuteNonQuery();
            }
        }

        public void Save(Product product)
        {
            using (var connection = new SqlConnection(SqlConst.ConnectionToShopString))
            {
                connection.Open();
                var command = new SqlCommand($"INSERT INTO [dbo].[Product]([CategoryId],[LocationId],[StateId],[UserId],[Name],[Description],[Price],[CreationDate],[LastModifiedDate],[IsArchive]) VALUES(@categoryId, @locationId, @stateId, @authorId, @productName, @description, @price, @creationDate, @lastModifiedDate, @isArchive)", connection);
                command.Parameters.AddWithValue("@categoryId", product.Category.Id);
                command.Parameters.AddWithValue("@locationId", product.Location.Id);
                command.Parameters.AddWithValue("@stateId", product.State.Id);
                command.Parameters.AddWithValue("@authorId", product.Author.Id);
                command.Parameters.AddWithValue("@productName", product.Name);
                command.Parameters.AddWithValue("@description", product.Description);
                command.Parameters.AddWithValue("@price", product.Price);
                command.Parameters.AddWithValue("@creationDate", product.CreationDate);
                command.Parameters.AddWithValue("@lastModifiedDate", product.LastModifiedDate);
                command.Parameters.AddWithValue("@isArchive", 0);
                command.ExecuteNonQuery();
            }
        }

        public int GetIdByProduct(Product product)
        {
            using (var connection = new SqlConnection(SqlConst.ConnectionToShopString))
            {
                connection.Open();
                //StringBuilder builder = new StringBuilder($"SELECT [Product] WHERE [Name] = '{product.Name}'");
                //if(product.Price != default(decimal))
                //{
                //    builder.Append($" AND [Price] = {product.Price}");
                //}
                //if(product.Author.Id != default(int))
                //{
                //    builder.Append($" AND [UserId] = {product.Author.Id}");
                //}
                //if(product.Description != default(string))
                //{
                //    builder.Append($" AND [Description] = '{product.Description}'");
                //}

                var command = new SqlCommand("SELECT * FROM [Product] WHERE [Name] = @name AND [UserId] = @userId AND [Description] = @description", connection);
                command.Parameters.AddWithValue("@name", product.Name);
                command.Parameters.AddWithValue("@userId", product.Author.Id);
                command.Parameters.AddWithValue("@description", product.Description);
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                return (int)reader["Id"];
            }
        }
    }
}
