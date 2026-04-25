using ShoppedTogetherHaztartAszok.Dnn.Dnn.ShoppedTogetherHaztartAszok.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

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

        public List<RelatedProductStat> GetTopRelatedProducts(int productId)
        {
            var result = new List<RelatedProductStat>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            SELECT
                CASE 
                    WHEN stp.ProductAId = @ProductId THEN pbTr.ProductName
                    ELSE paTr.ProductName
                END AS ProductName,
                stp.TogetherCount
            FROM dbo.ShoppedTogetherProductPairs stp
            INNER JOIN dbo.hcc_Product pa ON pa.Id = stp.ProductAId
            INNER JOIN dbo.hcc_Product pb ON pb.Id = stp.ProductBId
            INNER JOIN dbo.hcc_ProductTranslations paTr ON paTr.ProductId = pa.bvin
            INNER JOIN dbo.hcc_ProductTranslations pbTr ON pbTr.ProductId = pb.bvin
            WHERE @ProductId IN (stp.ProductAId, stp.ProductBId)
              AND stp.TogetherCount > 0
              AND paTr.Culture = 'en-US'
              AND pbTr.Culture = 'en-US'
            ORDER BY stp.TogetherCount DESC", conn);

                cmd.Parameters.AddWithValue("@ProductId", productId);

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new RelatedProductStat
                        {
                            ProductName = rdr["ProductName"].ToString(),
                            TogetherCount = Convert.ToInt32(rdr["TogetherCount"])
                        });
                    }
                }
            }

            return result;
        }

        public List<ProductPairStat> GetTopProductPairs(int limit)
        {
            var result = new List<ProductPairStat>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
           SELECT TOP (@Limit)
                paTr.ProductName AS ProductAName,
                pbTr.ProductName AS ProductBName,
                stp.TogetherCount
            FROM dbo.ShoppedTogetherProductPairs stp
            INNER JOIN dbo.hcc_Product pa ON pa.Id = stp.ProductAId
            INNER JOIN dbo.hcc_Product pb ON pb.Id = stp.ProductBId
            INNER JOIN dbo.hcc_ProductTranslations paTr ON paTr.ProductId = pa.bvin
            INNER JOIN dbo.hcc_ProductTranslations pbTr ON pbTr.ProductId = pb.bvin
            WHERE paTr.Culture = 'en-US'
              AND pbTr.Culture = 'en-US'
            ORDER BY stp.TogetherCount DESC, paTr.ProductName, pbTr.ProductName", conn);

                cmd.Parameters.AddWithValue("@Limit", limit);

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new ProductPairStat
                        {
                            ProductAName = rdr["ProductAName"].ToString(),
                            ProductBName = rdr["ProductBName"].ToString(),
                            TogetherCount = Convert.ToInt32(rdr["TogetherCount"])
                        });
                    }
                }
            }

            return result;
        }

    }
}