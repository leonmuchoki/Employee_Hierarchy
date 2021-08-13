using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EmployeeHierarchy
{
    public class Employees
    {

        List<Employee> employeesData;

        public Employees(string csv_file_name)
        {
            validateCSV(csv_file_name);
            employeesData = new List<Employee>();
        }

        /// <summary>
        /// first CSV column contains the id of the employee, the second one contains the id of the
        ///manager, and the third one contains the employee’s salary.
        /// </summary>
        /// <param name="csv_file_name"></param>
        private (bool isValid, string return_message) validateCSV(string csv_file_name)
        {
            bool isValid = true;
            string[] rows;
            HashSet<int> employeez = new HashSet<int>();
            HashSet<int> managers = new HashSet<int>();
            Dictionary<int, int> emp_manager_dict = new Dictionary<int, int>();
            int ceo_id = 0;

            try
            {
                using (StreamReader sr = new StreamReader(csv_file_name))
                {
                    while (sr.Peek() >= 0)
                    {
                        rows = sr.ReadLine().Replace("\"", "").Split(',');
                        int currEmpSalary = 0;
                        int validator = 0;
                        //salary should be valid integer
                        if (!int.TryParse(rows[2], out validator))
                        {
                            return (false, "salary does not have valid integer for employee id: " + rows[0]);
                        }
                        else
                        {
                            currEmpSalary = validator;
                        }

                        //One employee does not report to more than one manager.
                        int currEmp = int.Parse(rows[0]);
                        if (employeez.Contains(currEmp))
                        {
                            return (false, "employee already has a manager. Employee id is: " + rows[0]);
                        }
                        else
                        {
                            employeez.Add(currEmp);
                        }

                        //There is only one CEO, i.e. only one employee with no manager.
                        if (currEmp == ceo_id)
                        {
                            return (false, "employee already has a manager. Employee id is: " + rows[0]);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(rows[1].Trim())) ceo_id = currEmp;
                        }

                        int currManager = int.Parse(rows[1]);
                        emp_manager_dict.Add(currEmp, currManager);
                        managers.Add(currManager);

                        Employee employee = new Employee()
                        {
                            employee_id = currEmp,
                            manager_id = currManager,
                            employee_salary = currEmpSalary
                        };
                        employeesData.Add(employee);
                    }
                }

                /*There is no circular reference, i.e. a first employee reporting 
                         * to a second employee that is also under the first employee. */
                bool has_circular_reference = false;
                foreach (var em in emp_manager_dict)
                {
                    int currEmp = em.Key;
                    int currManager = em.Value;
                    int mng = emp_manager_dict[currManager];
                    if (currEmp == mng) has_circular_reference = true;
                }

                if (has_circular_reference) return (false, "has circular reference");

                /*
                 There is no manager that is not an employee, i.e. 
                 all managers are also listed in the employee column.
                */
                if (!managers.SetEquals(employeez)) return (false, "there exists a manager not an employee");

                return (isValid, "good to go");
            }
            catch(Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        ///  returns the salary budget from the specified manager.
        /// </summary>
        public int getManagerSalaryBudget(int manager_id)
        {
            int total_budget = 0;
            try
            {
                var budgetEmployees = employeesData.Where(x => x.manager_id == manager_id).Select(x => x.employee_salary).Sum();
                var currManagerSalary = employeesData.Where(x => x.employee_id == manager_id).Select(x => x.employee_salary).FirstOrDefault();
                total_budget = budgetEmployees + currManagerSalary;
            }
            catch(Exception ex)
            {
                //
            }
            return total_budget;
        }
    }

    public class Employee
    {
        public int employee_id { get; set; }
        public int manager_id { get; set; }
        public int employee_salary { get; set; }
    }
}
