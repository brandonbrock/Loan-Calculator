using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Runtime.Serialization.Formatters.Binary;

namespace Coursework
{
    [Serializable]
    public struct Loans
    {
        public int month;
        public double ammountRemaining;
        public double monthlyPayment;
        public double interestPaid;
        public double paymentTowards;

        public Loans(int m, double aR, double mP, double iP, double pT)
        {
            this.month = m;
            this.ammountRemaining = aR;
            this.monthlyPayment = mP;
            this.interestPaid = iP;
            this.paymentTowards = pT;
        }
    }

    public class LoanMain
    {
        static void Main()
        {

            //public variables
            bool sucesss;
            double propertyPrice, interestRate;
            int loanDuration;
            string input;
            string[] inputYes = ConfigurationManager.AppSettings["inputYes"].Split(',');
            string[] inputNo = ConfigurationManager.AppSettings["inputNo"].Split(',');

            //opening message
            var title = ConfigurationManager.AppSettings["title"];
            Console.WriteLine($"{title}");

            //asking if user has loan already
            string prompt = "\nDo you have a loan already?(Y/N) ";
            Console.Write(prompt);
            while (true)
            {
                input = Console.ReadLine();
                //if true then file will open from folder
                if (inputYes.Contains(input))
                {
                    Load();
                }
                //if not true user will continue to get a loan quote 
                else if (inputNo.Contains(input))
                {
                    //gets the property price ammount
                    Console.Write("\nProperty Price: ");
                    input = Console.ReadLine();
                    sucesss = double.TryParse(input, out propertyPrice);
                    while (!sucesss)
                    {
                        Console.WriteLine("Please enter the correct input type e.g. 1000");
                        Console.Write("\nProperty Price: ");
                        input = Console.ReadLine();
                        sucesss = double.TryParse(input, out propertyPrice);
                    }

                    //the annual yearly interest rate
                    Console.Write("\nInterest rate: ");
                    input = Console.ReadLine();
                    sucesss = double.TryParse(input, out interestRate);
                    while (!sucesss)
                    {
                        Console.WriteLine("Please enter the correct input type e.g. 2.5");
                        Console.Write("\nInterest rate: ");
                        input = Console.ReadLine();
                        sucesss = double.TryParse(input, out interestRate);
                    }

                    //the number of years the loan will be active for
                    Console.Write("\nLoan Duration in Years: ");
                    input = Console.ReadLine();

                    sucesss = Int32.TryParse(input, out loanDuration);
                    while (!sucesss)
                    {
                        Console.WriteLine("Please enter the correct input type e.g. 10");
                        Console.Write("\nLoan Duration in Years: ");
                        input = Console.ReadLine();
                        sucesss = Int32.TryParse(input, out loanDuration);
                    }

                    //send the data to the loan class below
                    LoanCalculations loan = new LoanCalculations(propertyPrice, interestRate, loanDuration);

                    //displays the required information
                    loan.LoanTable(inputYes, inputNo);
                    break;
                }
                else
                {
                    Console.WriteLine("Invlaid Input. Try Again!");
                    Console.Write(prompt);
                }
            }
        }

        static void Load()
        {
            //the text file that is being loaded to the console program changeable in app.config
            string uploadFilename = ConfigurationManager.AppSettings["uploadFilename"];
            if (!File.Exists(uploadFilename))
            {
                Console.WriteLine("\nNo File in Directory, Try Again!");
            }
            else
            {
                BinaryFormatter bf = new BinaryFormatter();
                //open the file storing the data
                StreamReader upload = new StreamReader(uploadFilename);
                var lines = File.ReadAllLines(uploadFilename);

                //read the data from the file
                for (int i = 0; i < lines.Length; ++i)
                {
                    string line = upload.ReadLine();
                    Console.WriteLine(line);
                }

                /*search function
                Console.WriteLine("\nWhat Month would you like to look at: ");
                int search = int.Parse(Console.ReadLine());*/

                //close the file
                upload.Close();
                Console.WriteLine("\nUpload Completed! Press Enter to Exit....");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }
    }
    public class LoanCalculations
    {

        //variables
        double propertyPrice, interestRate;
        int loanDuration;

        //constant integer for max months by year
        const int max_months_year = 12;

        //the text file that is being saved to the console program changeable in app.config
        string saveFilename = ConfigurationManager.AppSettings["saveFilename"];

        //takes the ammount from user input class loan
        public LoanCalculations(double amount, double rate, int years)
        {
            propertyPrice = amount;
            interestRate = (rate / 100.0) / max_months_year;
            loanDuration = years;
        }
        //calculates the monthly payment with the loan duration in sections of a 12 month peroid
        public double MonthlyPayment()
        {
            int months = loanDuration * max_months_year;

            //return new monthly payment using math.pow to return interestRate as it is a double
            //formula used M = P[r(1+r)^n/((1+r)^n)-1)]
            return (propertyPrice * interestRate * Math.Pow(1 + interestRate, months)) / (Math.Pow(1 + interestRate, months) - 1);
        }
        //lists the loan in a table format of what is owed each month
        public void LoanTable(string[] inputYes, string[] inputNo)
        {
            double monthlyPayment = MonthlyPayment(); //calculates monthly payment
            double paymentTowards = double.Parse(ConfigurationManager.AppSettings["paymentTowards"]);
            double ammountRemaining = double.Parse(ConfigurationManager.AppSettings["ammountRemaining"]);
            double interestPaid = double.Parse(ConfigurationManager.AppSettings["interestPaid"]);
            double principal = propertyPrice;

            //month, payment amount, principal paid, interest paid, total interest paid, balance
            //the curly brackets is an Alignment Component to position to month, balance, installment, interest paid and principal
            Console.WriteLine("\n{0,5}{1,10}{2,10}{3,10}{4,15}", "Month ", "Balance ", "Installment ", "Interest Paid ", "Principal Paid ");

            for (int month = 1; month <= loanDuration * max_months_year; month++)
            {
                // Compute amount paid and new balance for each payment period
                interestPaid = principal * interestRate;
                paymentTowards = monthlyPayment - interestPaid;
                ammountRemaining = principal - paymentTowards;
                // Output the data item              
                Console.WriteLine("{0,-5}{1,10:C}{2,10:C}{3,10:C}{4,15:C}",
                  month, ammountRemaining, monthlyPayment, interestPaid, paymentTowards);
                // Update the balance
                principal = ammountRemaining;
            }

            //asks user if they want their information saved
            string prompt = "\nWould you like to save this information to a text file(y/n) ";
            Console.Write(prompt);
            while (true)
            {
                string input = Console.ReadLine();
                if (inputYes.Contains(input))
                {
                    using (StreamWriter save = new StreamWriter(saveFilename))
                    {
                        save.WriteLine("\n{0,5}{1,10}{2,10}{3,10}{4,15}", "Month ", "Balance ", "Installment ", "Interest Paid ", "Principal Paid ");
                        BinaryFormatter bf = new BinaryFormatter();
                        for (int month = 1; month <= loanDuration * max_months_year; month++)
                        {
                            // Compute amount paid and new balance for each payment period
                            interestPaid = principal * interestRate;
                            paymentTowards = monthlyPayment - interestPaid;
                            ammountRemaining = principal - paymentTowards;
                            // Output the data item              
                            save.WriteLine("{0,-5}{1,10:C}{2,10:C}{3,10:C}{4,15:C}",
                              month, ammountRemaining, monthlyPayment, interestPaid, paymentTowards);
                            // Update the balance
                            principal = ammountRemaining;
                        }
                        //close the file
                        save.Close();
                    }
                    Console.WriteLine("\nSave Complete, Press Enter to Exit...");
                    break;

                }
                else if (inputNo.Contains(input))
                {
                    Console.WriteLine("\nPress Enter to Exit...");
                    Console.ReadLine();
                    break;
                }
                else
                {
                    Console.WriteLine("Invlaid Input. Try Again!");
                    Console.Write(prompt);
                }
            }

        }
    }
}