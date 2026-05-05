using FoodOutlet.Models;
using MySql.Data.MySqlClient;

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
                                // FIX Problem 1: Use null coalescing operator
                                role.role_name = rst["role_name"]?.ToString() ?? "";

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
                    SELECT r.id, r.registration_name AS name, r.email, r.phone_no, r.address, r.role_id, r.photo,
                           ro.role_name,
                           CASE WHEN rs.registration_id IS NULL THEN 'In Service' ELSE 'Resign' END AS status
                    FROM registrations r
                    LEFT JOIN roles ro ON r.role_id = ro.id
                    LEFT JOIN resigns rs ON r.id = rs.registration_id
                    ORDER BY r.id";
                
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (MySqlDataReader rst = cmd.ExecuteReader())
                    {
                        while (rst.Read())
                        {
                            // Simply retrieve the photo path from database
                            string photoPath = rst["photo"]?.ToString() ?? "";

                            staffList.Add(new Models.Staff
                            {
                                id = rst["id"] == DBNull.Value ? 0 : Convert.ToInt32(rst["id"]),
                                name = rst["name"]?.ToString() ?? "",
                                email = rst["email"]?.ToString() ?? "",
                                phone_no = rst["phone_no"]?.ToString() ?? "",
                                address = rst["address"]?.ToString() ?? "",
                                role_id = rst["role_id"] == DBNull.Value ? 0 : Convert.ToInt32(rst["role_id"]),
                                photo = photoPath,
                                role_name = rst["role_name"]?.ToString() ?? "",
                                status = rst["status"]?.ToString() ?? ""
                            });
                        }
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
                        var imagePath = rdr["recipe_img"]?.ToString() ?? "";
                        imagePath = NormalizeImagePath(imagePath);

                        list.Add(new
                        {
                            id = rdr["id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["id"]),
                            recipe_name = rdr["recipe_name"]?.ToString() ?? "",
                            category_id = rdr["category_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["category_id"]),
                            recipe_img = imagePath,
                            description = rdr["description"]?.ToString() ?? "",
                            price = rdr["price"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["price"]),
                            category_name = rdr["category_name"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return list;
        }

        // GetAllInventories - returns one row per recipe, aggregates any duplicate inventory rows
        public List<dynamic> GetAllInventories()
        {
            var list = new List<dynamic>();
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                // FIXED SQL SYNTAX: Added spaces after "inventories" and "r.id"
                string sql = @"
SELECT r.id AS recipe_id, r.recipe_name, r.recipe_img, 
       COALESCE(i.stock_qty, 0) AS stock_qty, 
       COALESCE(i.inventory_id, 0) AS inventory_id,
       i.created_at, i.updated_at 
FROM recipes r 
LEFT JOIN (
    SELECT recipe_id, SUM(stock_qty) AS stock_qty, MAX(id) AS inventory_id, 
           MAX(created_at) AS created_at, MAX(updated_at) AS updated_at
    FROM inventories
    GROUP BY recipe_id
) i ON i.recipe_id = r.id
ORDER BY r.recipe_name, r.id";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var imagePath = rdr["recipe_img"] == DBNull.Value ? "" : rdr["recipe_img"].ToString();
                        imagePath = NormalizeImagePath(imagePath);

                        list.Add(new
                        {
                            inventory_id = rdr["inventory_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["inventory_id"]),
                            recipe_id = rdr["recipe_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["recipe_id"]),
                            recipe_name = rdr["recipe_name"]?.ToString() ?? "",
                            recipe_img = imagePath,
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
                    SELECT r.id, r.registration_name AS name, r.email, r.phone_no, r.address,
                           r.photo, ro.role_name,
                           rs.reason, rs.resign_at AS resign
                    FROM registrations r
                    INNER JOIN resigns rs ON r.id = rs.registration_id
                    JOIN roles ro ON ro.id = r.role_id
                    ORDER BY rs.resign_at DESC, rs.id DESC";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new
                        {
                            id        = rdr["id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["id"]),
                            name      = rdr["name"]?.ToString() ?? "",
                            email     = rdr["email"]?.ToString() ?? "",
                            phone_no  = rdr["phone_no"]?.ToString() ?? "",
                            address   = rdr["address"]?.ToString() ?? "",
                            photo     = rdr["photo"]?.ToString() ?? "",
                            role_name = rdr["role_name"]?.ToString() ?? "",
                            reason    = rdr["reason"]?.ToString() ?? "",
                            resign    = rdr["resign"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["resign"])
                        });
                    }
                }
            }
            return list;
        }

        #endregion


        #region Get and Set
        // FIX Problem 5: Change return type to nullable (Models.Role?)
        public Models.Role? GetRoleById(int id)
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

        // FIX Problem 10: Change return type to nullable (Models.Staff?)
        // FIXED: Added photo field to retrieval
        public Models.Staff? GetStaffById(int id)
        {
            using (MySqlConnection conn = _connectionFactory.CreateConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand("SELECT id,registration_name AS name,email,password_hash,birth_of_date,phone_no,address,role_id,photo FROM registrations WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (MySqlDataReader rst = cmd.ExecuteReader())
                    {
                        if (rst.Read())
                        {
                            string photoData = "";
                            
                            // Safely retrieve photo data
                            int photoOrdinal = rst.GetOrdinal("photo");
                            if (!rst.IsDBNull(photoOrdinal))
                            {
                                try
                                {
                                    photoData = rst.GetString(photoOrdinal) ?? "";
                                }
                                catch
                                {
                                    // If string conversion fails, try as byte array
                                    try
                                    {
                                        byte[] photoBytes = rst.GetFieldValue<byte[]>(photoOrdinal);
                                        if (photoBytes != null && photoBytes.Length > 0)
                                        {
                                            photoData = Convert.ToBase64String(photoBytes);
                                            photoData = "data:image/jpeg;base64," + photoData;
                                        }
                                    }
                                    catch
                                    {
                                        photoData = "";
                                    }
                                }
                            }

                            return new Models.Staff
                            {
                                id = rst["id"] == DBNull.Value ? 0 : Convert.ToInt32(rst["id"]),
                                name = rst["name"]?.ToString() ?? "",
                                email = rst["email"]?.ToString() ?? "",
                                password = rst["password_hash"]?.ToString() ?? "",
                                birth_of_date = rst["birth_of_date"] == DBNull.Value ? null : Convert.ToDateTime(rst["birth_of_date"]),
                                phone_no = rst["phone_no"]?.ToString() ?? "",
                                address = rst["address"]?.ToString() ?? "",
                                role_id = rst["role_id"] == DBNull.Value ? 0 : Convert.ToInt32(rst["role_id"]),
                                photo = photoData
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
                    
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(staf.name))
                    {
                        msg.message = "Error: Staff name is required";
                        return msg;
                    }
                    if (string.IsNullOrWhiteSpace(staf.email))
                    {
                        msg.message = "Error: Email is required";
                        return msg;
                    }
                    if (staf.role_id <= 0)
                    {
                        msg.message = "Error: Role must be selected";
                        return msg;
                    }
                    if (string.IsNullOrWhiteSpace(staf.password) && staf.id == 0)
                    {
                        msg.message = "Error: Password is required for new staff";
                        return msg;
                    }

                    if (staf.id > 0)
                    {
                        // UPDATE existing staff
                        using (MySqlCommand cmd = new MySqlCommand(
                            "UPDATE registrations SET registration_name=@name, email=@email, birth_of_date=@birth_of_date, " +
                            "password_hash=@password_hash, phone_no=@phone_no, address=@address, role_id=@role_id" +
                            (string.IsNullOrEmpty(staf.photo) ? "" : ", photo=@photo") +
                            " WHERE id=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", staf.id);
                            cmd.Parameters.AddWithValue("@name", staf.name ?? "");
                            cmd.Parameters.AddWithValue("@email", staf.email ?? "");
                            cmd.Parameters.AddWithValue("@birth_of_date", staf.birth_of_date ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@password_hash", string.IsNullOrEmpty(staf.password) ? DBNull.Value : (object)staf.password);
                            cmd.Parameters.AddWithValue("@phone_no", staf.phone_no ?? "");
                            cmd.Parameters.AddWithValue("@address", staf.address ?? "");
                            cmd.Parameters.AddWithValue("@role_id", staf.role_id);
                            if (!string.IsNullOrEmpty(staf.photo))
                            {
                                cmd.Parameters.AddWithValue("@photo", staf.photo);
                            }
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            msg.message = rowsAffected > 0 ? "Success" : "Error: Staff not found";
                        }
                    }
                    else
                    {
                        // INSERT new staff
                        using (MySqlCommand cmd = new MySqlCommand(
                            "INSERT INTO registrations (registration_name, email, birth_of_date, password_hash, phone_no, address, role_id, photo, created_at) " +
                            "VALUES (@name, @email, @birth_of_date, @password_hash, @phone_no, @address, @role_id, @photo, NOW())", conn))
                        {
                            cmd.Parameters.AddWithValue("@name", staf.name ?? "");
                            cmd.Parameters.AddWithValue("@email", staf.email ?? "");
                            cmd.Parameters.AddWithValue("@birth_of_date", staf.birth_of_date ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@password_hash", staf.password ?? "");
                            cmd.Parameters.AddWithValue("@phone_no", staf.phone_no ?? "");
                            cmd.Parameters.AddWithValue("@address", staf.address ?? "");
                            cmd.Parameters.AddWithValue("@role_id", staf.role_id);
                            cmd.Parameters.AddWithValue("@photo", staf.photo ?? "");
                            
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

        // FIX Problem 11: Change return type to nullable (Recipe?)
        public Recipe? GetRecipeById(int id)
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

        // FIX Problem 12: Change return type to nullable (Models.Inventory?)
        public Models.Inventory? GetInventoryById(int id)
        {
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

        // SetInventory - if inserting (id==0) but an inventory row for the recipe exists, update it instead of inserting duplicate
        public Message SetInventory(Models.Inventory inv)
        {
            var msg = new Message();
            if (inv == null || inv.recipe_id <= 0)
            {
                msg.message = "Invalid payload";
                return msg;
            }

            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        // EDIT: id > 0 => replace quantity for that inventory row
                        if (inv.id > 0)
                        {
                            using (var cmd = new MySqlCommand("UPDATE inventories SET stock_qty = @qty, recipe_id = @rid, updated_at = NOW() WHERE id = @id", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@qty", inv.stock_qty);
                                cmd.Parameters.AddWithValue("@rid", inv.recipe_id);
                                cmd.Parameters.AddWithValue("@id", inv.id);
                                int affected = cmd.ExecuteNonQuery();
                                tx.Commit();
                                msg.message = affected > 0 ? "Updated" : "Error: Not found";
                                return msg;
                            }
                        }

                        // ADD: id == 0 => add quantity to existing row for recipe_id or insert new
                        using (var cmdFind = new MySqlCommand("SELECT id, stock_qty FROM inventories WHERE recipe_id = @rid LIMIT 1 FOR UPDATE", conn, tx))
                        {
                            cmdFind.Parameters.AddWithValue("@rid", inv.recipe_id);
                            using (var rdr = cmdFind.ExecuteReader())
                            {
                                if (rdr.Read())
                                {
                                    int existingId = rdr["id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["id"]);
                                    int existingQty = rdr["stock_qty"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["stock_qty"]);
                                    rdr.Close();

                                    int newQty = existingQty + inv.stock_qty;
                                    using (var cmdUpd = new MySqlCommand("UPDATE inventories SET stock_qty = @qty, updated_at = NOW() WHERE id = @id", conn, tx))
                                    {
                                        cmdUpd.Parameters.AddWithValue("@qty", newQty);
                                        cmdUpd.Parameters.AddWithValue("@id", existingId);
                                        cmdUpd.ExecuteNonQuery();
                                    }

                                    tx.Commit();
                                    msg.message = "Updated";
                                    return msg;
                                }
                                else
                                {
                                    // no existing row, close reader then insert
                                    rdr.Close();
                                    using (var cmdIns = new MySqlCommand("INSERT INTO inventories (recipe_id, stock_qty, created_at) VALUES (@rid, @qty, NOW())", conn, tx))
                                    {
                                        cmdIns.Parameters.AddWithValue("@rid", inv.recipe_id);
                                        cmdIns.Parameters.AddWithValue("@qty", inv.stock_qty);
                                        cmdIns.ExecuteNonQuery();
                                    }

                                    tx.Commit();
                                    msg.message = "Success";
                                    return msg;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.message = "Error: " + ex.Message;
                return msg;
            }
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

        /// <summary>
        /// Normalizes recipe image path to ensure correct format: /uploads/recipes/{filename}
        /// Handles legacy paths and ensures consistency
        /// </summary>
        private string NormalizeImagePath(string? imagePath)  // ← Changed to string? (nullable)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return "";

            // Remove leading/trailing whitespace
            imagePath = imagePath.Trim();

            // Already in correct format
            if (imagePath.StartsWith("/uploads/recipes/"))
                return imagePath;

            // Fix legacy path format: /img/recipes/ → /uploads/recipes/
            if (imagePath.StartsWith("/img/recipes/"))
                return imagePath.Replace("/img/recipes/", "/uploads/recipes/");

            // If it's just a filename, prepend the correct path
            if (!imagePath.Contains("/"))
                return $"/uploads/recipes/{imagePath}";

            // Default: return as-is (might be a full URL or already correct)
            return imagePath;
        }

        #region Order Management

        /// <summary>
        /// FK on Order / order_detail references table_lists.id (not tables.id).
        /// Maps a QR-registered table_number to a table_lists row, creating one if needed.
        /// </summary>
        private int ResolveTableListIdForOrder(int tableNumber, MySqlConnection conn)
        {
            using (var verify = new MySqlCommand("SELECT 1 FROM `tables` WHERE table_number = @tn LIMIT 1", conn))
            {
                verify.Parameters.AddWithValue("@tn", tableNumber);
                var tableRowExists = verify.ExecuteScalar() != null;
                if (!tableRowExists)
                    return 0;
            }

            try
            {
                using (var byNum = new MySqlCommand("SELECT id FROM `table_lists` WHERE table_number = @tn LIMIT 1", conn))
                {
                    byNum.Parameters.AddWithValue("@tn", tableNumber);
                    var o = byNum.ExecuteScalar();
                    if (o != null)
                        return Convert.ToInt32(o);
                }
            }
            catch (MySqlException)
            {
                // Schemas that only have table_name (no table_number column)
            }

            string[] nameCandidates = { tableNumber.ToString(), $"Table {tableNumber}" };
            foreach (var name in nameCandidates)
            {
                using (var cmd = new MySqlCommand("SELECT id FROM `table_lists` WHERE table_name = @n LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("@n", name);
                    var o = cmd.ExecuteScalar();
                    if (o != null)
                        return Convert.ToInt32(o);
                }
            }

            using (var ins = new MySqlCommand("INSERT INTO `table_lists` (table_name) VALUES (@n)", conn))
            {
                ins.Parameters.AddWithValue("@n", tableNumber.ToString());
                ins.ExecuteNonQuery();
            }

            using (var lid = new MySqlCommand("SELECT LAST_INSERT_ID()", conn))
            {
                return Convert.ToInt32(lid.ExecuteScalar());
            }
        }

        private int ResolveInitialStatusId(MySqlConnection conn)
        {
            using (var byId = new MySqlCommand("SELECT id FROM `status` WHERE id = 1 LIMIT 1", conn))
            {
                var existing = byId.ExecuteScalar();
                if (existing != null)
                    return 1;
            }

            using (var byName = new MySqlCommand("SELECT id FROM `status` WHERE LOWER(name) IN ('pending','new','ordered') ORDER BY id ASC LIMIT 1", conn))
            {
                var named = byName.ExecuteScalar();
                if (named != null)
                {
                    return Convert.ToInt32(named);
                }
            }

            using (var ins = new MySqlCommand("INSERT INTO `status` (name) VALUES ('Pending')", conn))
            {
                ins.ExecuteNonQuery();
            }
            using (var lid = new MySqlCommand("SELECT LAST_INSERT_ID()", conn))
            {
                return Convert.ToInt32(lid.ExecuteScalar());
            }
        }

        /// <summary>
        /// Call after inserting into `tables` so Order FK (table_lists) has a matching row.
        /// </summary>
        public void EnsureTableListRowForRegisteredTable(int tableNumber)
        {
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    var id = ResolveTableListIdForOrder(tableNumber, conn);
                    if (id == 0)
                        Console.WriteLine("EnsureTableListRowForRegisteredTable: no tables row for #" + tableNumber);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EnsureTableListRowForRegisteredTable: " + ex.Message);
            }
        }

        /// <summary>
        /// Create order with items
        /// </summary>
        // Tracks whether the order_id column migration has run this app-lifetime.
        private static bool _orderDetailMigrated = false;
        private static readonly object _migrateLock = new();

        /// <summary>
        /// Adds order_id column to order_detail if it doesn't exist yet.
        /// Must be called OUTSIDE any active transaction (ALTER TABLE causes implicit commit).
        /// </summary>
        private void EnsureOrderDetailOrderId(MySqlConnection conn)
        {
            if (_orderDetailMigrated) return;
            lock (_migrateLock)
            {
                if (_orderDetailMigrated) return;
                try
                {
                    using var check = new MySqlCommand(
                        "SELECT COUNT(*) FROM information_schema.columns " +
                        "WHERE table_schema = DATABASE() AND table_name = 'order_detail' AND column_name = 'order_id'", conn);
                    if (Convert.ToInt32(check.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand(
                            "ALTER TABLE order_detail ADD COLUMN order_id INT NULL DEFAULT NULL", conn);
                        alter.ExecuteNonQuery();
                        Console.WriteLine("order_detail.order_id column added.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EnsureOrderDetailOrderId error: " + ex.Message);
                }
                _orderDetailMigrated = true;
            }
        }

        public Message CreateOrder(int tableNumber, List<Models.OrderItem> items)
        {
            var msg = new Message();
            if (items == null || items.Count == 0)
            {
                msg.message = "No items to order";
                return msg;
            }

            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    EnsureOrderDetailOrderId(conn); // must run before transaction
                    using (var tx = conn.BeginTransaction())
                    {
                        int tableId = ResolveTableListIdForOrder(tableNumber, conn);
                        if (tableId == 0)
                        {
                            msg.message = "Invalid table number";
                            tx.Rollback();
                            return msg;
                        }
                        var statusId = ResolveInitialStatusId(conn);

                        // Merge duplicated recipe lines in one order request to validate/deduct correctly.
                        var requiredByRecipe = new Dictionary<int, int>();
                        foreach (var item in items)
                        {
                            if (item == null || item.recipe_id <= 0 || item.qty <= 0)
                            {
                                msg.message = "Invalid order item";
                                tx.Rollback();
                                return msg;
                            }

                            if (!requiredByRecipe.ContainsKey(item.recipe_id))
                                requiredByRecipe[item.recipe_id] = 0;
                            requiredByRecipe[item.recipe_id] += item.qty;
                        }

                        // Validate stock with row locks and stage deductions.
                        var deductions = new Dictionary<int, List<(int inventoryId, int deductQty)>>();
                        foreach (var req in requiredByRecipe)
                        {
                            int recipeId = req.Key;
                            int neededQty = req.Value;

                            var rows = new List<(int inventoryId, int stockQty)>();
                            using (var cmd = new MySqlCommand("SELECT id, stock_qty FROM inventories WHERE recipe_id = @rid FOR UPDATE", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", recipeId);
                                using (var rdr = cmd.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        rows.Add((
                                            Convert.ToInt32(rdr["id"]),
                                            rdr["stock_qty"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["stock_qty"])
                                        ));
                                    }
                                }
                            }

                            int totalStock = rows.Sum(r => r.stockQty);
                            if (totalStock < neededQty)
                            {
                                string recipeName = recipeId.ToString();
                                try
                                {
                                    using (var nameCmd = new MySqlCommand("SELECT recipe_name FROM recipes WHERE id = @rid LIMIT 1", conn, tx))
                                    {
                                        nameCmd.Parameters.AddWithValue("@rid", recipeId);
                                        var n = nameCmd.ExecuteScalar();
                                        if (n != null) recipeName = n.ToString()!;
                                    }
                                }
                                catch { }
                                msg.message = $"Not enough stock for \"{recipeName}\". Available: {totalStock}, Requested: {neededQty}";
                                tx.Rollback();
                                return msg;
                            }

                            int remaining = neededQty;
                            var plan = new List<(int inventoryId, int deductQty)>();
                            foreach (var row in rows)
                            {
                                if (remaining <= 0) break;
                                if (row.stockQty <= 0) continue;
                                int take = Math.Min(row.stockQty, remaining);
                                plan.Add((row.inventoryId, take));
                                remaining -= take;
                            }
                            deductions[recipeId] = plan;
                        }

                        // Create order record after stock validation passes.
                        using (var cmd = new MySqlCommand("INSERT INTO `Order` (table_id, status) VALUES (@tid, 'Pending')", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@tid", tableId);
                            cmd.ExecuteNonQuery();
                        }

                        // Keep for future relation/reporting if needed.
                        int orderId = 0;
                        using (var cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", conn, tx))
                        {
                            orderId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Insert each line item — stamp order_id so items are tied to this order only.
                        foreach (var item in items)
                        {
                            using (var cmd = new MySqlCommand(
                                "INSERT INTO order_detail (table_id, recipe_id, qty, status_id, order_id) " +
                                "VALUES (@tid, @rid, @qty, @sid, @oid)", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@tid", tableId);
                                cmd.Parameters.AddWithValue("@rid", item.recipe_id);
                                cmd.Parameters.AddWithValue("@qty", item.qty);
                                cmd.Parameters.AddWithValue("@sid", statusId);
                                cmd.Parameters.AddWithValue("@oid", orderId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Apply stock deductions.
                        foreach (var recipePlan in deductions)
                        {
                            foreach (var step in recipePlan.Value)
                            {
                                using (var cmd = new MySqlCommand("UPDATE inventories SET stock_qty = stock_qty - @dq, updated_at = NOW() WHERE id = @id", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@dq", step.deductQty);
                                    cmd.Parameters.AddWithValue("@id", step.inventoryId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();
                        msg.message = "Success";
                        return msg;
                    } // transaction scope
                }
            }
            catch (Exception ex)
            {
                msg.message = "Error: " + ex.Message;
                Console.WriteLine("Order creation error: " + ex.Message);
            }

            return msg;
        }

        #endregion

        #region Order Workflow

        /// <summary>
        /// Returns orders (with item summaries) whose status matches one of the supplied values.
        /// Joins table_lists for the table label and order_detail + recipes for items.
        /// NOTE: order_detail is linked by table_id (not order_id) so this works correctly
        /// when one active order exists per table at a time — normal restaurant operation.
        /// </summary>
        public List<dynamic> GetOrdersWithItems(params string[] statuses)
        {
            var list = new List<dynamic>();
            if (statuses == null || statuses.Length == 0) return list;

            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    EnsureOrderDetailOrderId(conn); // ensure column exists before querying

                    var paramNames = statuses.Select((_, i) => $"@s{i}").ToArray();
                    string inClause = string.Join(", ", paramNames);

                    string sql = $@"
                        SELECT
                            o.id          AS order_id,
                            o.status,
                            o.created_at,
                            tl.table_name AS table_label,
                            GROUP_CONCAT(
                                CONCAT(r.recipe_name, ' x', od.qty)
                                ORDER BY od.id
                                SEPARATOR ', '
                            ) AS items_summary
                        FROM `Order` o
                        JOIN  table_lists  tl ON tl.id    = o.table_id
                        LEFT JOIN order_detail od ON od.order_id = o.id
                        LEFT JOIN recipes       r  ON r.id       = od.recipe_id
                        WHERE o.status IN ({inClause})
                        GROUP BY o.id, o.status, o.created_at, tl.table_name
                        ORDER BY o.created_at ASC";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        for (int i = 0; i < statuses.Length; i++)
                            cmd.Parameters.AddWithValue(paramNames[i], statuses[i]);

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                var summary = rdr["items_summary"]?.ToString() ?? "—";
                                list.Add(new
                                {
                                    order_id      = Convert.ToInt32(rdr["order_id"]),
                                    status        = rdr["status"]?.ToString() ?? "",
                                    created_at    = rdr["created_at"] == DBNull.Value
                                                        ? DateTime.MinValue
                                                        : Convert.ToDateTime(rdr["created_at"]),
                                    table_label   = rdr["table_label"]?.ToString() ?? "?",
                                    items_summary = summary,
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetOrdersWithItems error: " + ex.Message);
            }

            return list;
        }

        /// <summary>
        /// Returns all active orders for a given table (Pending / Approved / Ready / Served).
        /// Excludes Cleaning, Done, and Cancelled so the list resets once the cashier sends
        /// the table to clean.
        /// </summary>
        public List<dynamic> GetTableOrderHistory(int tableNumber)
        {
            var list = new List<dynamic>();
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    EnsureOrderDetailOrderId(conn);

                    const string sql = @"
                        SELECT
                            o.id                               AS order_id,
                            o.status,
                            o.created_at,
                            COALESCE(SUM(r.price * od.qty), 0) AS order_total,
                            GROUP_CONCAT(
                                CONCAT(r.recipe_name, ' x', od.qty)
                                ORDER BY od.id
                                SEPARATOR ', '
                            )                                  AS items_summary
                        FROM `Order` o
                        JOIN  table_lists  tl ON tl.id       = o.table_id
                        LEFT JOIN order_detail od ON od.order_id = o.id
                        LEFT JOIN recipes       r  ON r.id       = od.recipe_id
                        WHERE tl.table_name IN (@tnStr, @tnFmt)
                          AND o.status NOT IN ('Cleaning', 'Done', 'Cancelled')
                        GROUP BY o.id, o.status, o.created_at
                        ORDER BY o.created_at ASC";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@tnStr", tableNumber.ToString());
                        cmd.Parameters.AddWithValue("@tnFmt", $"Table {tableNumber}");

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                list.Add(new
                                {
                                    order_id      = Convert.ToInt32(rdr["order_id"]),
                                    status        = rdr["status"]?.ToString() ?? "",
                                    created_at    = rdr["created_at"] == DBNull.Value
                                                        ? DateTime.MinValue
                                                        : Convert.ToDateTime(rdr["created_at"]),
                                    order_total   = rdr["order_total"] == DBNull.Value
                                                        ? 0m
                                                        : Convert.ToDecimal(rdr["order_total"]),
                                    items_summary = rdr["items_summary"]?.ToString() ?? "—",
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTableOrderHistory error: " + ex.Message);
            }
            return list;
        }

        /// <summary>
        /// Groups all currently-Cleaning orders by table (same structure as GetServedByTable).
        /// The cleaner marks the whole table Done in one click.
        /// </summary>
        public List<dynamic> GetCleaningByTable()
        {
            var list = new List<dynamic>();
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    EnsureOrderDetailOrderId(conn);

                    const string sql = @"
                        SELECT
                            tl.table_name                                                              AS table_label,
                            GROUP_CONCAT(DISTINCT CAST(o.id AS CHAR) ORDER BY o.id SEPARATOR ',')      AS order_ids,
                            MIN(o.created_at)                                                          AS first_ordered_at,
                            GROUP_CONCAT(
                                CONCAT(r.recipe_name, ' x', od.qty)
                                ORDER BY od.id
                                SEPARATOR '|'
                            )                                                                          AS items_raw
                        FROM `Order` o
                        JOIN  table_lists  tl ON tl.id       = o.table_id
                        LEFT JOIN order_detail od ON od.order_id = o.id
                        LEFT JOIN recipes       r  ON r.id       = od.recipe_id
                        WHERE o.status = 'Cleaning'
                        GROUP BY tl.table_name, o.table_id
                        ORDER BY MIN(o.created_at) ASC";

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new
                            {
                                table_label      = rdr["table_label"]?.ToString() ?? "?",
                                order_ids        = rdr["order_ids"]?.ToString() ?? "",
                                first_ordered_at = rdr["first_ordered_at"] == DBNull.Value
                                                       ? DateTime.MinValue
                                                       : Convert.ToDateTime(rdr["first_ordered_at"]),
                                items_raw        = rdr["items_raw"]?.ToString() ?? "—",
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCleaningByTable error: " + ex.Message);
            }
            return list;
        }

        /// <summary>
        /// Groups all currently-Served orders by table. Each entry contains the table label,
        /// a comma-separated list of order IDs, the earliest order time, an aggregated items
        /// summary, and the total cost (sum of recipe price × qty across all orders for that
        /// table).
        /// </summary>
        public List<dynamic> GetServedByTable()
        {
            var list = new List<dynamic>();
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    EnsureOrderDetailOrderId(conn);

                    const string sql = @"
                        SELECT
                            tl.table_name                                                              AS table_label,
                            GROUP_CONCAT(DISTINCT CAST(o.id AS CHAR) ORDER BY o.id SEPARATOR ',')      AS order_ids,
                            MIN(o.created_at)                                                          AS first_ordered_at,
                            COALESCE(SUM(r.price * od.qty), 0)                                         AS total_cost,
                            GROUP_CONCAT(
                                CONCAT(r.recipe_name, ' x', od.qty)
                                ORDER BY od.id
                                SEPARATOR '|'
                            )                                                                          AS items_raw
                        FROM `Order` o
                        JOIN  table_lists  tl ON tl.id       = o.table_id
                        LEFT JOIN order_detail od ON od.order_id = o.id
                        LEFT JOIN recipes       r  ON r.id       = od.recipe_id
                        WHERE o.status = 'Served'
                        GROUP BY tl.table_name, o.table_id
                        ORDER BY MIN(o.created_at) ASC";

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new
                            {
                                table_label      = rdr["table_label"]?.ToString() ?? "?",
                                order_ids        = rdr["order_ids"]?.ToString() ?? "",
                                first_ordered_at = rdr["first_ordered_at"] == DBNull.Value
                                                       ? DateTime.MinValue
                                                       : Convert.ToDateTime(rdr["first_ordered_at"]),
                                total_cost       = rdr["total_cost"] == DBNull.Value
                                                       ? 0m
                                                       : Convert.ToDecimal(rdr["total_cost"]),
                                items_raw        = rdr["items_raw"]?.ToString() ?? "—",
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetServedByTable error: " + ex.Message);
            }
            return list;
        }

        /// <summary>
        /// Moves a set of orders (by their IDs) to a new status in one statement.
        /// </summary>
        public Models.Message UpdateMultipleOrderStatus(List<int> orderIds, string newStatus)
        {
            var msg = new Models.Message();
            if (orderIds == null || orderIds.Count == 0)
            {
                msg.message = "Error: No order IDs provided.";
                return msg;
            }
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    var paramNames = orderIds.Select((_, i) => $"@id{i}").ToArray();
                    string sql = $"UPDATE `Order` SET status = @s, updated_at = NOW() WHERE id IN ({string.Join(",", paramNames)})";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@s", newStatus);
                        for (int i = 0; i < orderIds.Count; i++)
                            cmd.Parameters.AddWithValue(paramNames[i], orderIds[i]);
                        int rows = cmd.ExecuteNonQuery();
                        msg.message = rows > 0 ? "Success" : "Error: No orders were updated.";
                    }
                }
            }
            catch (Exception ex)
            {
                msg.message = "Error: " + ex.Message;
                Console.WriteLine("UpdateMultipleOrderStatus error: " + ex.Message);
            }
            return msg;
        }

        /// <summary>
        /// Moves an order to a new status. Returns "Success" or an error string.
        /// </summary>
        public Models.Message UpdateOrderStatus(int orderId, string newStatus)
        {
            var msg = new Models.Message();
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(
                        "UPDATE `Order` SET status = @s, updated_at = NOW() WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@s",   newStatus);
                        cmd.Parameters.AddWithValue("@id",  orderId);
                        int rows = cmd.ExecuteNonQuery();
                        msg.message = rows > 0 ? "Success" : "Error: Order not found";
                    }
                }
            }
            catch (Exception ex)
            {
                msg.message = "Error: " + ex.Message;
                Console.WriteLine("UpdateOrderStatus error: " + ex.Message);
            }
            return msg;
        }

        #endregion

        #region Staff Self-Service

        public Models.Staff? GetMyStatus(int staffId)
        {
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    const string sql = @"
                        SELECT r.id, r.registration_name AS name, r.email, r.phone_no, r.address, r.photo,
                               ro.role_name,
                               CASE WHEN rs.registration_id IS NULL THEN 'In Service' ELSE 'Resigned' END AS status
                        FROM registrations r
                        JOIN roles ro ON ro.id = r.role_id
                        LEFT JOIN resigns rs ON rs.registration_id = r.id
                        WHERE r.id = @id
                        LIMIT 1";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", staffId);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                return new Models.Staff
                                {
                                    id        = Convert.ToInt32(rdr["id"]),
                                    name      = rdr["name"]?.ToString() ?? "",
                                    email     = rdr["email"]?.ToString() ?? "",
                                    phone_no  = rdr["phone_no"]?.ToString() ?? "",
                                    address   = rdr["address"]?.ToString() ?? "",
                                    photo     = rdr["photo"]?.ToString() ?? "",
                                    role_name = rdr["role_name"]?.ToString() ?? "",
                                    status    = rdr["status"]?.ToString() ?? "",
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetMyStatus error: " + ex.Message);
            }
            return null;
        }

        public bool HasResigned(int staffId)
        {
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM resigns WHERE registration_id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", staffId);
                        var count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HasResigned error: " + ex.Message);
            }
            return false;
        }

        public Models.Message SubmitResign(int staffId, string reason)
        {
            var msg = new Models.Message();
            try
            {
                if (HasResigned(staffId))
                {
                    msg.message = "Error: You have already submitted a resignation.";
                    return msg;
                }

                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(
                        "INSERT INTO resigns (registration_id, reason, resign_at) VALUES (@id, @reason, NOW())", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", staffId);
                        cmd.Parameters.AddWithValue("@reason", reason ?? "");
                        cmd.ExecuteNonQuery();
                        msg.message = "Success";
                    }
                }
            }
            catch (Exception ex)
            {
                msg.message = "Error: " + ex.Message;
                Console.WriteLine("SubmitResign error: " + ex.Message);
            }
            return msg;
        }

        #endregion

        #region Auth

        public Models.Staff? LoginStaff(string email, string password)
        {
            try
            {
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    const string sql = @"
                        SELECT r.id, r.registration_name AS name, r.email, r.photo,
                               ro.id AS role_id, ro.role_name
                        FROM registrations r
                        JOIN roles ro ON ro.id = r.role_id
                        WHERE r.email = @email AND r.password_hash = @password
                        LIMIT 1";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (var rst = cmd.ExecuteReader())
                        {
                            if (rst.Read())
                            {
                                return new Models.Staff
                                {
                                    id        = rst["id"] != DBNull.Value ? Convert.ToInt32(rst["id"]) : 0,
                                    name      = rst["name"]?.ToString() ?? "",
                                    email     = rst["email"]?.ToString() ?? "",
                                    photo     = rst["photo"]?.ToString() ?? "",
                                    role_id   = rst["role_id"] != DBNull.Value ? Convert.ToInt32(rst["role_id"]) : 0,
                                    role_name = rst["role_name"]?.ToString() ?? "",
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoginStaff error: " + ex.Message);
            }

            return null;
        }

        #endregion
    }
}