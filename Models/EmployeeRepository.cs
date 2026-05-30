using Microsoft.Data.SqlClient;

namespace WebApplication1.Models
{
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DBCS");
        }

        // READ
        public List<Employee> GetEmployees()
        {
            List<Employee> employees = new();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            string query = "SELECT * FROM Employee";

            SqlCommand cmd = new SqlCommand(query, con);

            con.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                employees.Add(new Employee
                {
                    Id = Convert.ToInt32(dr["Id"]),
                    Name = dr["Name"].ToString(),
                    Email = dr["Email"].ToString(),
                    Salary = Convert.ToDecimal(dr["Salary"])
                });
            }

            return employees;
        }

        // INSERT
        public void AddEmployee(Employee emp)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string query =
                @"INSERT INTO Employee(Name,Email,Salary)
                  VALUES(@Name,@Email,@Salary)";

            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@Name", emp.Name);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // GET BY ID
        public Employee GetEmployeeById(int id)
        {
            Employee emp = new Employee();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            SqlCommand cmd =
                new SqlCommand(
                    "SELECT * FROM Employee WHERE Id=@Id", con);

            cmd.Parameters.AddWithValue("@Id", id);

            con.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                emp.Id = Convert.ToInt32(dr["Id"]);
                emp.Name = dr["Name"].ToString();
                emp.Email = dr["Email"].ToString();
                emp.Salary = Convert.ToDecimal(dr["Salary"]);
            }

            return emp;
        }

        // UPDATE
        public void UpdateEmployee(Employee emp)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string query =
                @"UPDATE Employee
                  SET Name=@Name,
                      Email=@Email,
                      Salary=@Salary
                  WHERE Id=@Id";

            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@Id", emp.Id);
            cmd.Parameters.AddWithValue("@Name", emp.Name);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // DELETE
        public void DeleteEmployee(int id)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            SqlCommand cmd =
                new SqlCommand(
                    "DELETE FROM Employee WHERE Id=@Id", con);

            cmd.Parameters.AddWithValue("@Id", id);

            con.Open();
            cmd.ExecuteNonQuery();
        }
    }
}