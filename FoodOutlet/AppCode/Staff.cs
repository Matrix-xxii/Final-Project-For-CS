using MySql.Data.MySqlClient;
using FoodOutlet.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Linq.Expressions;

namespace FoodOutlet.AppCode
{                       
    public class Staff
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public Staff(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        #region GetAll
        public List<Models.Role> GetAllRoles()
        {
            List<Models.Role> roles = new List<Models.Role>();

            try
            {
                using (MySqlConnection conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand("SELECT id, role_name FROM roles ORDER BY id", conn))
                    {
                        using (MySqlDataReader rst = cmd.ExecuteReader())
                        {
                            while (rst.Read())
                            {
                                Models.Role role = new Models.Role();

                                role.id = Convert.ToInt32(rst["id"]);
                                role.role_name = rst["role_name"].ToString();

                                roles.Add(role);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ROLE ERROR: " + e.Message);
            }

            return roles;
        }

        public List<Models.Staff> GetAllStaffs()
        {
            List<Models.Staff> staffList = new List<Models.Staff>();
            using (MySqlConnection conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                string query = @"
                    SELECT r.id, r.registration_name AS name, r.email, r.phone_no, r.address, r.role_id,
                           CASE WHEN rs.registration_id IS NULL THEN 'In Service' ELSE 'Resign' END AS status
                    FROM registrations r
                    LEFT JOIN resigns rs ON r.id = rs.registration_id
                    ORDER BY r.id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader rst = cmd.ExecuteReader())
                {
                    while (rst.Read())
                    {
                        staffList.Add(new Models.Staff
                        {
                            id = rst["id"] == DBNull.Value ? 0 : Convert.ToInt32(rst["id"]),
                            name = rst["name"]?.ToString() ?? "",
                            email = rst["email"] == DBNull.Value ? "" : rst["email"].ToString(),
                            phone_no = rst["phone_no"] == DBNull.Value ? "" : rst["phone_no"].ToString(),
                            address = rst["address"] == DBNull.Value ? "" : rst["address"].ToString(),
                            role_id = rst["role_id"] == DBNull.Value ? 0 : Convert.ToInt32(rst["role_id"]),
                            status = rst["status"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return staffList;
        }

        public List<Models.Category> GetAllCategories()
        {
            var list = new List<Models.Category>();
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id, category_name FROM categories ORDER BY id", conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new Models.Category
                        {
                            id = rdr["id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["id"]),
                            category_name = rdr["category_name"]?.ToString() ?? ""
                        });
                    }
                }
            }

            return list;
        }

        public List<dynamic> GetAllRecipes()
        {
            var list = new List<dynamic>();
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                string sql = @"
            SELECT r.id, r.recipe_name, r.category_id, r.recipe_img, r.description, r.price, c.category_name
            FROM recipes r
            LEFT JOIN categories c ON r.category_id = c.id
            ORDER BY r.id";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new
                        {
                            id = rdr["id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["id"]),
                            recipe_name = rdr["recipe_name"]?.ToString() ?? "",
                            category_id = rdr["category_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["category_id"]),
                            recipe_img = rdr["recipe_img"]?.ToString() ?? "",
                            description = rdr["description"]?.ToString() ?? "",
                            price = rdr["price"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["price"]),
                            category_name = rdr["category_name"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return list;
        }

        public List<dynamic> GetAllInventories()
        {
            var list = new List<dynamic>();
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                // Return one row per recipe, with inventory information when present.
                string sql = @"
            SELECT r.id AS recipe_id,
                   r.recipe_name,
                   r.recipe_img,
                   COALESCE(i.stock_qty, 0) AS stock_qty,
                   i.id AS inventory_id,
                   i.created_at,
                   i.updated_at
            FROM recipes r
            LEFT JOIN inventories i ON i.recipe_id = r.id
            ORDER BY r.recipe_name, r.id";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new
                        {
                            inventory_id = rdr["inventory_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["inventory_id"]),
                            recipe_id = rdr["recipe_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["recipe_id"]),
                            recipe_name = rdr["recipe_name"] == DBNull.Value ? "" : rdr["recipe_name"].ToString(),
                            recipe_img = rdr["recipe_img"] == DBNull.Value ? "" : rdr["recipe_img"].ToString(),
                            stock_qty = rdr["stock_qty"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["stock_qty"]),
                            created_at = rdr["created_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["created_at"]),
                            updated_at = rdr["updated_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["updated_at"])
                        });
                    }
                }
            }
            return list;
        }
        public List<dynamic> GetResignRecords()
        {
            var list = new List<dynamic>();
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT r.id, r.registration_name AS name, r.email, r.phone_no, r.address, r.role_id,
                           rs.reason, rs.resign_at AS resign
                    FROM registrations r
                    INNER JOIN resigns rs ON r.id = rs.registration_id
                    ORDER BY (rs.resign_at IS NOT NULL) DESC, rs.resign_at DESC, rs.id DESC";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new
                        {
                            id = rdr["id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["id"]),
                            name = rdr["name"]?.ToString() ?? "",
                            email = rdr["email"] == DBNull.Value ? "" : rdr["email"].ToString(),
                            phone_no = rdr["phone_no"] == DBNull.Value ? "" : rdr["phone_no"].ToString(),
                            address = rdr["address"] == DBNull.Value ? "" : rdr["address"].ToString(),
                            role_id = rdr["role_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["role_id"]),
                            reason = rdr["reason"] == DBNull.Value ? "" : rdr["reason"].ToString(),
                            resign = rdr["resign"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["resign"])
                        });
                    }
                }
            }
            return list;
        }

        #endregion


        #region Get and Set
        public Models.Role GetRoleById(int id)
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id, role_name FROM roles WHERE id = @id LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            return new Models.Role
                            {
                                id = rdr["id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["id"]),
                                role_name = rdr["role_name"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }
            return null;
        }
        
        public Message SetRole(Models.Role role)
        {
            var msg = new Message();
            if (role == null)
            {
                msg.message = "Invalid role";
                return msg;
            }

            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();

                    if (role.id > 0)
                    {
                        using (var cmd = new MySqlCommand("UPDATE roles SET role_name = @role_name WHERE id = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@role_name", role.role_name ?? "");
                            cmd.Parameters.AddWithValue("@id", role.id);
                            cmd.ExecuteNonQuery();
                        }
                        msg.message = "Updated";
                    }
                    else
                    {
                        using (var cmd = new MySqlCommand("INSERT INTO roles (role_name) VALUES (@role_name)", conn))
                        {
                            cmd.Parameters.AddWithValue("@role_name", role.role_name ?? "");
                            cmd.ExecuteNonQuery();
                        }
                        msg.message = "Success";
                    }
                }
            }
            catch (Exception ex)
            {
                msg.message = "Error: " + ex.Message;
            }

            return msg;
        }

        public Models.Staff GetStaffById(int id)
        {
            using (MySqlConnection conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand("SELECT id,registration_name AS name,email,password_hash,birth_of_date,phone_no,address,role_id FROM registrations WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (MySqlDataReader rst = cmd.ExecuteReader())
                    {
                        if (rst.Read())
                        {
                            return new Models.Staff
                            {
                                id = rst["id"] == DBNull.Value ? 0 : Convert.ToInt32(rst["id"]),
                                name = rst["name"]?.ToString() ?? "",
                                email = rst["email"] == DBNull.Value ? "" : rst["email"].ToString(),
                                password = rst["password_hash"] == DBNull.Value ? "" : rst["password_hash"].ToString(),
                                birth_of_date = rst["birth_of_date"] == DBNull.Value ? null : Convert.ToDateTime(rst["birth_of_date"]),
                                phone_no = rst["phone_no"] == DBNull.Value ? "" : rst["phone_no"].ToString(),
                                address = rst["address"] == DBNull.Value ? "" : rst["address"].ToString(),
                                role_id = rst["role_id"] == DBNull.Value ? 0 : Convert.ToInt32(rst["role_id"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public Message SetStaff(Models.Staff staf)
        {
            Message msg = new Message();
            try
            {
                using (MySqlConnection conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    if (staf.id > 0)
                    {
                        using (MySqlCommand cmd = new MySqlCommand("UPDATE registrations SET registration_name=@name,email=@email,birth_of_date=@birth_of_date,password_hash=@password_hash,phone_no=@phone_no,address=@address,role_id=@role_id WHERE id=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", staf.id);
                            cmd.Parameters.AddWithValue("@name", staf.name);
                            cmd.Parameters.AddWithValue("@email", staf.email);
                            cmd.Parameters.AddWithValue("@birth_of_date", staf.birth_of_date);
                            cmd.Parameters.AddWithValue("@password_hash", staf.password);
                            cmd.Parameters.AddWithValue("@phone_no", staf.phone_no);
                            cmd.Parameters.AddWithValue("@address", staf.address);
                            cmd.Parameters.AddWithValue("@role_id", staf.role_id);
                            cmd.ExecuteNonQuery();
                            msg.message = "Updated";
                        }
                    }
                    else
                    {
                        using (MySqlCommand cmd = new MySqlCommand("INSERT INTO registrations (registration_name,email,birth_of_date,password_hash,phone_no,address,role_id) VALUES (@name,@email,@birth_of_date,@password_hash,@phone_no,@address,@role_id)", conn))
                        {
                            cmd.Parameters.AddWithValue("@name", staf.name);
                            cmd.Parameters.AddWithValue("@email", staf.email);
                            cmd.Parameters.AddWithValue("@birth_of_date", staf.birth_of_date);
                            cmd.Parameters.AddWithValue("@password_hash", staf.password);
                            cmd.Parameters.AddWithValue("@phone_no", staf.phone_no);
                            cmd.Parameters.AddWithValue("@address", staf.address);
                            cmd.Parameters.AddWithValue("@role_id", staf.role_id);
                            cmd.ExecuteNonQuery();
                            msg.message = "Success";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                msg.message = "Error: " + e.Message;
            }
            return msg;
        }

        public Message SetCategory(Models.Category cat)
        {
            Message msg = new Message();
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    if (cat.id > 0)
                    {
                        using (var cmd = new MySqlCommand("UPDATE categories SET category_name=@name WHERE id=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", cat.id);
                            cmd.Parameters.AddWithValue("@name", cat.category_name);
                            cmd.ExecuteNonQuery();
                            msg.message = "Success";
                        }
                    }
                    else
                    {
                        using (var cmd = new MySqlCommand("INSERT INTO categories (category_name) VALUES (@name)", conn))
                        {
                            cmd.Parameters.AddWithValue("@name", cat.category_name);
                            cmd.ExecuteNonQuery();
                            msg.message = "Success";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                msg.message = "Error: " + e.Message;
            }
            return msg;
        }

        public Message SetRecipe(Recipe r)
        {
            Message msg = new Message();
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    if (r.id > 0)
                    {
                        using (var cmd = new MySqlCommand("UPDATE recipes SET recipe_name=@name, category_id=@catid, recipe_img=@img, description=@desc, price=@price WHERE id=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", r.id);
                            cmd.Parameters.AddWithValue("@name", r.recipe_name ?? "");
                            cmd.Parameters.AddWithValue("@catid", r.category_id);
                            cmd.Parameters.AddWithValue("@img", r.recipe_img ?? "");
                            cmd.Parameters.AddWithValue("@desc", r.description ?? "");
                            cmd.Parameters.AddWithValue("@price", r.price);
                            cmd.ExecuteNonQuery();
                            msg.message = "Success";
                        }
                    }
                    else
                    {
                        using (var cmd = new MySqlCommand("INSERT INTO recipes (recipe_name, category_id, recipe_img, description, price, created_at) VALUES (@name, @catid, @img, @desc, @price, NOW())", conn))
                        {
                            cmd.Parameters.AddWithValue("@name", r.recipe_name ?? "");
                            cmd.Parameters.AddWithValue("@catid", r.category_id);
                            cmd.Parameters.AddWithValue("@img", r.recipe_img ?? "");
                            cmd.Parameters.AddWithValue("@desc", r.description ?? "");
                            cmd.Parameters.AddWithValue("@price", r.price);
                            cmd.ExecuteNonQuery();
                            msg.message = "Success";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                msg.message = "Error: " + e.Message;
            }
            return msg;
        }

        public Recipe GetRecipeById(int id)
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                string sql = "SELECT id, recipe_name, category_id, recipe_img, description, price FROM recipes WHERE id=@id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            return new Recipe
                            {
                                id = rdr["id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["id"]),
                                recipe_name = rdr["recipe_name"]?.ToString() ?? "",
                                category_id = rdr["category_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["category_id"]),
                                recipe_img = rdr["recipe_img"]?.ToString() ?? "",
                                description = rdr["description"]?.ToString() ?? "",
                                price = rdr["price"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["price"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public Models.Inventory GetInventoryById(int id)
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                // placeholder to satisfy pattern; actual connection created below
            }

            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                string sql = @"SELECT id, recipe_id, stock_qty, created_at, updated_at FROM inventories WHERE id = @id LIMIT 1";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            return new Models.Inventory
                            {
                                id = rdr["id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["id"]),
                                recipe_id = rdr["recipe_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["recipe_id"]),
                                stock_qty = rdr["stock_qty"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["stock_qty"]),
                                created_at = rdr["created_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["created_at"]),
                                updated_at = rdr["updated_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["updated_at"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public Message SetInventory(Models.Inventory inv)
        {
            var msg = new Message();
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    if (inv.id > 0)
                    {
                        using (var cmd = new MySqlCommand("UPDATE inventories SET recipe_id=@rid, stock_qty=@qty, updated_at=NOW() WHERE id=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", inv.id);
                            cmd.Parameters.AddWithValue("@rid", inv.recipe_id);
                            cmd.Parameters.AddWithValue("@qty", inv.stock_qty);
                            cmd.ExecuteNonQuery();
                        }
                        msg.message = "Updated";
                    }
                    else
                    {
                        using (var cmd = new MySqlCommand("INSERT INTO inventories (recipe_id, stock_qty, created_at) VALUES (@rid, @qty, NOW())", conn))
                        {
                            cmd.Parameters.AddWithValue("@rid", inv.recipe_id);
                            cmd.Parameters.AddWithValue("@qty", inv.stock_qty);
                            cmd.ExecuteNonQuery();
                        }
                        msg.message = "Success";
                    }
                }
            }
            catch (Exception ex)
            {
                msg.message = "Error: " + ex.Message;
            }
            return msg;
        }
        #endregion

        #region Count
        public int GetStaffCount()
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM registrations", conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int GetCategoryCount()
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM categories", conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int GetRoleCount()
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM roles", conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int GetResignCount()
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM resigns", conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int GetRecipeCount()
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM recipes", conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        #endregion


        #region Update and Delete

        public Message UpdateStaff(Models.Staff staf)
        {
            Message msg = new Message();
            try
            {
                using (MySqlConnection conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("UPDATE registrations SET registration_name=@registration_name,birth_of_date=@birth_of_date,role_id=@role_id WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@registration_name", staf.name);
                        cmd.Parameters.AddWithValue("@birth_of_date", staf.birth_of_date);
                        cmd.Parameters.AddWithValue("@role_id", staf.role_id);
                        cmd.Parameters.AddWithValue("@id", staf.id);
                        cmd.ExecuteNonQuery();
                        msg.message = "Success";
                    }
                }
            }
            catch (Exception e)
            {
                msg.message = "Error: " + e.Message;
            }
            return msg;
        }

        public Message DeleteStaff(int id)
        {
            Message msg = new Message();
            try
            {
                using (MySqlConnection conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("DELETE FROM registrations WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                        msg.message = "Success";
                    }
                }
            }
            catch (Exception e)
            {
                msg.message = "Error: " + e.Message;
            }
            return msg;
        }

        public Message DeleteRole(int id)
        {
            var msg = new Message();

            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("DELETE FROM roles WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                msg.message = "Success";
            }
            catch (Exception ex)
            {
                msg.message = "Error: " + ex.Message;
            }

            return msg;
        }

        public Message DeleteCategory(int id)
        {
            Message msg = new Message();
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("DELETE FROM categories WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                        msg.message = "Success";
                    }
                }
            }
            catch (Exception e)
            {
                msg.message = "Error: " + e.Message;
            }
            return msg;
        }

        public Message DeleteRecipe(int id)
        {
            Message msg = new Message();
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();

                    using (var tx = conn.BeginTransaction())
                    {
                        using (var cmdInv = new MySqlCommand("DELETE FROM inventories WHERE recipe_id = @rid", conn, tx))
                        {
                            cmdInv.Parameters.AddWithValue("@rid", id);
                            cmdInv.ExecuteNonQuery();
                        }

                        using (var cmd = new MySqlCommand("DELETE FROM recipes WHERE id=@id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }

                    msg.message = "Success";
                }
            }
            catch (Exception e)
            {
                msg.message = "Error: " + e.Message;
            }
            return msg;
        }

        public Message DeleteInventory(int id)
        {
            var msg = new Message();
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("DELETE FROM inventories WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                msg.message = "Success";
            }
            catch (Exception ex)
            {
                msg.message = "Error: " + ex.Message;
            }
            return msg;
        }

        #endregion

    }
}