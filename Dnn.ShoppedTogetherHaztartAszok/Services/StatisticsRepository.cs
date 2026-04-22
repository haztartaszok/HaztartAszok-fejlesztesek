using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using ShoppedTogetherHaztartAszok.Dnn.Dnn.ShoppedTogetherHaztartAszok.Models;

namespace Dnn.ShoppedTogetherHaztartAszok.Services
{
    public class StatisticsRepository
    {
        private readonly string _connectionString;

        public StatisticsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ProductItem> GetProducts()
        {
            var result = new List<ProductItem>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
                    SELECT 
                        p.Id,
                        pt.ProductName
                    FROM dbo.hcc_Product p
                    INNER JOIN dbo.hcc_ProductTranslations pt 
                        ON pt.ProductId = p.bvin
                    WHERE pt.Culture = 'en-US'
                    ORDER BY pt.ProductName", conn);

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new ProductItem
                        {
                            Id = Convert.ToInt32(rdr["Id"]),
                            ProductName = rdr["ProductName"].ToString()
                        });
                    }
                }
            }

            return result;
        }
    }
}