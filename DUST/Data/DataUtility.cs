using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DUST.Models;
using Microsoft.EntityFrameworkCore;
using DUST.Models.Enums;
using System.Diagnostics;

namespace DUST.Data
{
    public static class DataUtility
    {
        private static int company1Id;
        private static int company2Id;
        private static int company3Id;
        private static int company4Id;
        private static int company5Id;

        /// <summary>
        /// Gets the connection string either from a remote database url or from appsettings.json
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static string GetConnectionString(IConfiguration configuration)
        {
            var appSettingsConnectionString = configuration.GetConnectionString("DefaultConnection");
            var remoteDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            return string.IsNullOrEmpty(remoteDbUrl) ? appSettingsConnectionString : BuildConnectionString(remoteDbUrl);
        }

        public static string BuildConnectionString(string databaseUrl)
        {
            // Get an object representation of the URI
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };

            return connectionStringBuilder.ToString();
        }

        public static async Task ManageDataAsync(IHost host)
        {
            /* Inject services and create a database */

            using var serviceScope = host.Services.CreateScope();
            var serviceProvider = serviceScope.ServiceProvider;

            //Because the app is in the process of starting up, services must be injected this way
            var dbContextService = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManagerService = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManagerService = serviceProvider.GetRequiredService<UserManager<DUSTUser>>();
            IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Equivalent to update-database
            await dbContextService.Database.MigrateAsync();

            /* Seed Data */

            await SeedRolesAsync(roleManagerService);
            await SeedDefaultCompaniesAsync(dbContextService);
            await SeedDefaultProjectPriorityAsync(dbContextService);
            await SeedDefautProjectsAsync(dbContextService);
            await SeedDefaultUsersAsync(userManagerService, configuration);
            await SeedDemoUsersAsync(userManagerService, configuration);
            await SeedDefaultTicketTypeAsync(dbContextService);
            await SeedDefaultTicketStatusAsync(dbContextService);
            await SeedDefaultTicketPriorityAsync(dbContextService);
            await SeedDefaultTicketsAsync(dbContextService);
        }

        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Enum.GetNames(typeof(RolesEnum)))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        public static async Task SeedDefaultCompaniesAsync(ApplicationDbContext context)
        {
            try
            {
                IList<Company> defaultCompanies = new List<Company>() { 
                    new Company() {Name = "Company1", Description = "This is default company 1"},
                    new Company() {Name = "Company2", Description = "This is default company 2"},
                    new Company() {Name = "Company3", Description = "This is default company 3"},
                    new Company() {Name = "Company4", Description = "This is default company 4"},
                    new Company() {Name = "Company5", Description = "This is default company 5"},
                };

                List<string> existingCompanyNames = context.Companies.Select(c => c.Name).ToList();
                await context.Companies.AddRangeAsync(defaultCompanies.Where(c => !existingCompanyNames.Contains(c.Name)));
                await context.SaveChangesAsync();

                company1Id = context.Companies.FirstOrDefault(c => c.Name == "Company1").Id;
                company2Id = context.Companies.FirstOrDefault(c => c.Name == "Company2").Id;
                company3Id = context.Companies.FirstOrDefault(c => c.Name == "Company3").Id;
                company4Id = context.Companies.FirstOrDefault(c => c.Name == "Company4").Id;
                company5Id = context.Companies.FirstOrDefault(c => c.Name == "Company5").Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding companies ***");
                Debug.WriteLine(ex.Message);                
                throw;
            }
        }

        public static async Task SeedDefaultProjectPriorityAsync(ApplicationDbContext context)
        {
            try
            {
                IList<ProjectPriority> projectPriorities = new List<ProjectPriority>();
                foreach (var priority in Enum.GetNames(typeof(ProjectPriorityEnum)))
                {
                    projectPriorities.Add(new ProjectPriority() { Name = priority });
                }
                List<string> existingProjectPriorities = context.ProjectPriorities.Select(p => p.Name).ToList();
                await context.ProjectPriorities.AddRangeAsync(projectPriorities.Where(p => !existingProjectPriorities.Contains(p.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding project priorities ***");
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public static async Task SeedDefautProjectsAsync(ApplicationDbContext context)
        {
            ProjectPriority low = context.ProjectPriorities.FirstOrDefault(p => p.Name == ProjectPriorityEnum.Low.ToString());
            ProjectPriority medium = context.ProjectPriorities.FirstOrDefault(p => p.Name == ProjectPriorityEnum.Medium.ToString());
            ProjectPriority high = context.ProjectPriorities.FirstOrDefault(p => p.Name == ProjectPriorityEnum.High.ToString());
            ProjectPriority urgent = context.ProjectPriorities.FirstOrDefault(p => p.Name == ProjectPriorityEnum.Urgent.ToString());

            try
            {
                IList<Project> defaultProjects = new List<Project> {
                    new Project()
                    {
                        CompanyId = company1Id,
                        Name = "Build a Portfolio Website",
                        Description = "Host a Portfolio website to showcase a minimum of 4 projects using HTML 5," +
                                      " CSS 3, Bootstrap 5.2 and JavaScript",
                        StartDate = new DateTime(2022,01,01),
                        EndDate = new DateTime(2023,07,30),
                        ProjectPriorityId = high.Id
                    },
                    new Project()
                    {
                        CompanyId = company2Id,
                        Name = "Build a Blog Web App",
                        Description = "Build a .NET Core MVC Blog Web App that allows users to create, update and maintain blogs.",
                        StartDate = new DateTime(2022,01,01),
                        EndDate = new DateTime(2023,02,26),
                        ProjectPriorityId = low.Id
                    },
                    new Project()
                    {
                        CompanyId = company1Id,
                        Name = "Build a Bug Tracker Web App",
                        Description = "Build a multi tennent,.NET Core MVC web app that allows users and clients to" +
                                      " track bugs and features. Implemented with identity and user roles.",
                        StartDate = new DateTime(2022,01,01),
                        EndDate = new DateTime(2023,01,20),
                        ProjectPriorityId = high.Id
                    },
                    new Project()
                    {
                        CompanyId = company2Id,
                        Name = "Build an Address Book Web App",
                        Description = "Build a .NET Core MVC web app that allows users to store and update contact information. " +
                                      "Must use a postgres database in its initial design",
                        StartDate = new DateTime(2023,03,01),
                        EndDate = new DateTime(2023,03,15),
                        ProjectPriorityId = low.Id
                    },
                    new Project()
                    {
                        CompanyId = company2Id,
                        Name = "Build a Mortgage Calculator Web App",
                        Description = "Build a Mortgage calculator using Angular and RxJs",
                        StartDate = new DateTime(2023,03,16),
                        EndDate = new DateTime(2023,03,16).AddMonths(2),
                        ProjectPriorityId = medium.Id
                    },
                };

                var existingProjects = context.Projects.Select(p => p.Name).ToList();
                await context.Projects.AddRangeAsync(defaultProjects.Where(p => !existingProjects.Contains(p.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding projects ***");
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public static async Task SeedDefaultUsersAsync(UserManager<DUSTUser> userManager, IConfiguration configuration)
        {
            /* ADMIN Company 1 */

            var admin1Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ADMIN1_EMAIL"))
                                    ? configuration["Admin1User:Email"]
                                    : Environment.GetEnvironmentVariable("ADMIN1_EMAIL");
            var admin1Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ADMIN1_PASSWORD"))
                                    ? configuration["Admin1User:Password"]
                                    : Environment.GetEnvironmentVariable("ADMIN1_PASSWORD");

            var defaultAdmin1 = new DUSTUser
            {
                UserName = admin1Email,
                Email = admin1Email,
                FirstName = "Angel",
                LastName = "Admin",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(defaultAdmin1.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(defaultAdmin1, admin1Password);
                    await userManager.AddToRoleAsync(defaultAdmin1, RolesEnum.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default admin user ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* ADMIN Company 2 */

            var admin2Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ADMIN2_EMAIL"))
                                   ? configuration["Admin2User:Email"]
                                   : Environment.GetEnvironmentVariable("ADMIN2_EMAIL");
            var admin2Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ADMIN2_PASSWORD"))
                                    ? configuration["Admin2User:Password"]
                                    : Environment.GetEnvironmentVariable("ADMIN2_PASSWORD");

            var defaultAdmin2 = new DUSTUser
            {
                UserName = admin2Email,
                Email = admin2Email,
                FirstName = "Johnson",
                LastName = "Admin",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(defaultAdmin2.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(defaultAdmin2, admin2Password);
                    await userManager.AddToRoleAsync(defaultAdmin2, RolesEnum.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default admin user ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Project Manager Company 1 */

            var pm1Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PM1_EMAIL"))
                                   ? configuration["PM1User:Email"]
                                   : Environment.GetEnvironmentVariable("PM1_EMAIL");
            var pm1Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PM1_PASSWORD"))
                                    ? configuration["PM1User:Password"]
                                    : Environment.GetEnvironmentVariable("PM1_PASSWORD");

            var defaultPm1 = new DUSTUser
            {
                UserName = pm1Email,
                Email = pm1Email,
                FirstName = "Carlos",
                LastName = "PM",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(defaultPm1.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(defaultPm1, pm1Password);
                    await userManager.AddToRoleAsync(defaultPm1, RolesEnum.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default project manager ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Project Manager Company 2 */

            var pm2Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PM2_EMAIL"))
                                   ? configuration["PM2User:Email"]
                                   : Environment.GetEnvironmentVariable("PM2_EMAIL");
            var pm2Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PM2_PASSWORD"))
                                    ? configuration["PM2User:Password"]
                                    : Environment.GetEnvironmentVariable("PM2_PASSWORD");

            var defaultPm2User = new DUSTUser
            {
                UserName = pm2Email,
                Email = pm2Email,
                FirstName = "Eric",
                LastName = "PM",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(defaultPm2User.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(defaultPm2User, pm2Password);
                    await userManager.AddToRoleAsync(defaultPm2User, RolesEnum.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default project manager ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer 1 */

            var dev1Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEV1_EMAIL"))
                                   ? configuration["Dev1User:Email"]
                                   : Environment.GetEnvironmentVariable("DEV1_EMAIL");
            var dev1Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEV1_PASSWORD"))
                                   ? configuration["Dev1User:Password"]
                                   : Environment.GetEnvironmentVariable("DEV1_PASSWORD");

            var dev1User = new DUSTUser
            {
                UserName = dev1Email,
                Email = dev1Email,
                FirstName = "Sandro",
                LastName = "Dev",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(dev1User.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(dev1User, dev1Password);
                    await userManager.AddToRoleAsync(dev1User, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer 1 ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer 2 */

            var dev2Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEV2_EMAIL"))
                                   ? configuration["Dev2User:Email"]
                                   : Environment.GetEnvironmentVariable("DEV2_EMAIL");
            var dev2Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEV2_PASSWORD"))
                                   ? configuration["Dev2User:Password"]
                                   : Environment.GetEnvironmentVariable("DEV2_PASSWORD");

            var dev2User = new DUSTUser
            {
                UserName = dev2Email,
                Email = dev2Email,
                FirstName = "David",
                LastName = "Dev",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(dev2User.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(dev2User, dev2Password);
                    await userManager.AddToRoleAsync(dev2User, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer 2 ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer 3 */

            var dev3Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEV3_EMAIL"))
                                   ? configuration["Dev3User:Email"]
                                   : Environment.GetEnvironmentVariable("DEV3_EMAIL");
            var dev3Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEV3_PASSWORD"))
                                   ? configuration["Dev3User:Password"]
                                   : Environment.GetEnvironmentVariable("DEV3_PASSWORD");

            var dev3User = new DUSTUser
            {
                UserName = dev3Email,
                Email = dev3Email,
                FirstName = "Alex",
                LastName = "Dev",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(dev3User.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(dev3User, dev3Password);
                    await userManager.AddToRoleAsync(dev3User, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer 3 ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer 4 */

            var dev4Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEV4_EMAIL"))
                                   ? configuration["Dev4User:Email"]
                                   : Environment.GetEnvironmentVariable("DEV4_EMAIL");
            var dev4Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEV4_PASSWORD"))
                                   ? configuration["Dev4User:Password"]
                                   : Environment.GetEnvironmentVariable("DEV4_PASSWORD");

            var dev4User = new DUSTUser
            {
                UserName = dev4Email,
                Email = dev4Email,
                FirstName = "Alex",
                LastName = "Dev",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(dev4User.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(dev4User, dev4Password);
                    await userManager.AddToRoleAsync(dev4User, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer 4 ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Submitter 1 */

            var submitter1Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUB1_EMAIL"))
                                   ? configuration["Submitter1User:Email"]
                                   : Environment.GetEnvironmentVariable("SUB1_EMAIL");
            var submitter1Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUB1_PASSWORD"))
                                   ? configuration["Submitter1User:Password"]
                                   : Environment.GetEnvironmentVariable("SUB1_PASSWORD");

            var submitter1User = new DUSTUser
            {
                UserName = submitter1Email,
                Email = submitter1Email,
                FirstName = "Rene",
                LastName = "Sub",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(submitter1User.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(submitter1User, submitter1Password);
                    await userManager.AddToRoleAsync(submitter1User, RolesEnum.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default submitter 1 ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Submitter 2 */

            var submitter2Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUB2_EMAIL"))
                                   ? configuration["Submitter2User:Email"]
                                   : Environment.GetEnvironmentVariable("SUB2_EMAIL");
            var submitter2Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUB2_PASSWORD"))
                                   ? configuration["Submitter2User:Password"]
                                   : Environment.GetEnvironmentVariable("SUB2_PASSWORD");

            var submitter2User = new DUSTUser
            {
                UserName = submitter2Email,
                Email = submitter2Email,
                FirstName = "Jef",
                LastName = "Sub",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(submitter2User.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(submitter2User, submitter2Password);
                    await userManager.AddToRoleAsync(submitter2User, RolesEnum.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default submitter 2 ***");
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public static async Task SeedDemoUsersAsync(UserManager<DUSTUser> userManager, IConfiguration configuration)
        {
            /* Admin Demo 1*/

            var adminDemo1Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ADMIN_DEMO1_EMAIL"))
                                        ? configuration["AdminDemo1User:Email"]
                                        : Environment.GetEnvironmentVariable("ADMIN_DEMO1_EMAIL");
            var adminDemo1Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ADMIN_DEMO1_PASSWORD"))
                                        ? configuration["AdminDemo1User:Password"]
                                        : Environment.GetEnvironmentVariable("ADMIN_DEMO1_PASSWORD");

            var adminDemo1User = new DUSTUser
            {
                UserName = adminDemo1Email,
                Email = adminDemo1Email,
                FirstName = "Bob",
                LastName = "Spiegel",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(adminDemo1User.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(adminDemo1User, adminDemo1Password);
                    await userManager.AddToRoleAsync(adminDemo1User, RolesEnum.Admin.ToString());
                    await userManager.AddToRoleAsync(adminDemo1User, RolesEnum.DemoUser.ToString()); 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**** ERROR ****");
                Debug.WriteLine("Error Seeding admin demo 1 account");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Admin Demo 2*/

            var adminDemo2Email = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ADMIN_DEMO2_EMAIL"))
                                        ? configuration["AdminDemo2User:Email"]
                                        : Environment.GetEnvironmentVariable("ADMIN_DEMO2_EMAIL");
            var adminDemo2Password = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ADMIN_DEMO2_PASSWORD"))
                                        ? configuration["AdminDemo2User:Password"]
                                        : Environment.GetEnvironmentVariable("ADMIN_DEMO2_PASSWORD");

            var adminDemo2User = new DUSTUser
            {
                UserName = adminDemo2Email,
                Email = adminDemo2Email,
                FirstName = "Patrick",
                LastName = "Holtz",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(adminDemo2User.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(adminDemo2User, adminDemo2Password);
                    await userManager.AddToRoleAsync(adminDemo2User, RolesEnum.Admin.ToString());
                    await userManager.AddToRoleAsync(adminDemo2User, RolesEnum.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**** ERROR ****");
                Debug.WriteLine("Error Seeding admin demo 2 account");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Project Manager Demo */

            var pmDemoEmail = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PM_DEMO_EMAIL"))
                                       ? configuration["PMDemoUser:Email"]
                                       : Environment.GetEnvironmentVariable("PM_DEMO_EMAIL");
            var pmDemoPassword = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PM_DEMO_PASSWORD"))
                                        ? configuration["PMDemoUser:Password"]
                                        : Environment.GetEnvironmentVariable("PM_DEMO_PASSWORD");

            var pmDemoUser = new DUSTUser
            {
                UserName = pmDemoEmail,
                Email = pmDemoEmail,
                FirstName = "Steven",
                LastName = "Hanz",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(pmDemoUser.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(pmDemoUser, pmDemoPassword);
                    await userManager.AddToRoleAsync(pmDemoUser, RolesEnum.ProjectManager.ToString());
                    await userManager.AddToRoleAsync(pmDemoUser, RolesEnum.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**** ERROR ****");
                Debug.WriteLine("Error Seeding project manager demo account");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer Demo */

            var devDemoEmail = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEV_DEMO_EMAIL"))
                                       ? configuration["DevDemoUser:Email"]
                                       : Environment.GetEnvironmentVariable("DEV_DEMO_EMAIL");
            var devDemoPassword = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEV_DEMO_PASSWORD"))
                                        ? configuration["DevDemoUser:Password"]
                                        : Environment.GetEnvironmentVariable("DEV_DEMO_PASSWORD");

            var devDemoUser = new DUSTUser
            {
                UserName = devDemoEmail,
                Email = devDemoEmail,
                FirstName = "Juan",
                LastName = "Hernandez",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(devDemoUser.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(devDemoUser, devDemoPassword);
                    await userManager.AddToRoleAsync(devDemoUser, RolesEnum.Developer.ToString());
                    await userManager.AddToRoleAsync(devDemoUser, RolesEnum.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**** ERROR ****");
                Debug.WriteLine("Error Seeding developer demo account");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Submitter Demo */

            var submitterDemoEmail = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUB_DEMO_EMAIL"))
                                       ? configuration["SubmitterDemoUser:Email"]
                                       : Environment.GetEnvironmentVariable("SUB_DEMO_EMAIL");
            var submitterDemoPassword = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUB_DEMO_PASSWORD"))
                                        ? configuration["SubmitterDemoUser:Password"]
                                        : Environment.GetEnvironmentVariable("SUB_DEMO_PASSWORD");

            var submitterDemoUser = new DUSTUser
            {
                UserName = submitterDemoEmail,
                Email = submitterDemoEmail,
                FirstName = "Gabriel",
                LastName = "Garcia",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(submitterDemoUser.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(submitterDemoUser, submitterDemoPassword);
                    await userManager.AddToRoleAsync(submitterDemoUser, RolesEnum.Submitter.ToString());
                    await userManager.AddToRoleAsync(submitterDemoUser, RolesEnum.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**** ERROR ****");
                Debug.WriteLine("Error Seeding submitter demo account");
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public static async Task SeedDefaultTicketTypeAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketType> ticketTypes = new List<TicketType>()
                {
                    new TicketType() {Name = TicketTypeEnum.NewDevelopment.ToString()},
                    new TicketType() {Name = TicketTypeEnum.WorkTask.ToString()},
                    new TicketType() {Name = TicketTypeEnum.Defect.ToString()},
                    new TicketType() {Name = TicketTypeEnum.ChangeRequest.ToString()},
                    new TicketType() {Name = TicketTypeEnum.Enhancement.ToString()},
                    new TicketType() {Name = TicketTypeEnum.GeneralTask.ToString()},
                };

                List<string> existingTicketTypes = context.TicketTypes.Select(t => t.Name).ToList();
                await context.TicketTypes.AddRangeAsync(ticketTypes.Where(t => !existingTicketTypes.Contains(t.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**** ERROR ****");
                Debug.WriteLine("Error seeding ticket types");
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public static async Task SeedDefaultTicketStatusAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketStatus> ticketStatutes = new List<TicketStatus>()
                {
                    new TicketStatus() { Name = TicketStatusEnum.New.ToString() },
                    new TicketStatus() { Name = TicketStatusEnum.Development.ToString() },
                    new TicketStatus() { Name = TicketStatusEnum.Testing.ToString() },
                    new TicketStatus() { Name = TicketStatusEnum.Closed.ToString() }
                };

                List<string> existingStatutes = context.TicketStatuses.Select(t => t.Name).ToList();

                await context.TicketStatuses.AddRangeAsync(ticketStatutes.Where(t => !existingStatutes.Contains(t.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**** ERROR ****");
                Debug.WriteLine("Error seeding ticket statutes");
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public static async Task SeedDefaultTicketPriorityAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketPriority> ticketPriorities = new List<TicketPriority>()
                {
                    new TicketPriority() { Name = TicketPriorityEnum.Low.ToString() },
                    new TicketPriority() { Name = TicketPriorityEnum.Medium.ToString() },
                    new TicketPriority() { Name = TicketPriorityEnum.High.ToString() },
                    new TicketPriority() { Name = TicketPriorityEnum.Urgent.ToString() }
                };

                List<string> existingPriorities = context.TicketPriorities.Select(t => t.Name).ToList();

                await context.TicketPriorities.AddRangeAsync(ticketPriorities.Where(t => !existingPriorities.Contains(t.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**** ERROR ****");
                Debug.WriteLine("Error seeding ticket priorities");
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public static async Task SeedDefaultTicketsAsync(ApplicationDbContext context)
        {
            // Get Project Ids
            int portfolioId = context.Projects.FirstOrDefault(p => p.Name == "Build a Portfolio Website").Id;
            int blogId = context.Projects.FirstOrDefault(p => p.Name == "Build a Blog Web App").Id;
            int bugTrackerId = context.Projects.FirstOrDefault(p => p.Name == "Build a Bug Tracker Web App").Id;
            int addressBookId = context.Projects.FirstOrDefault(p => p.Name == "Build an Address Book Web App").Id;
            int mortgageCalcId = context.Projects.FirstOrDefault(p => p.Name == "Build a Mortgage Calculator Web App").Id;

            // Get ticket type Ids
            int typeNewId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.NewDevelopment.ToString()).Id;
            int typeWorkTaskId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.WorkTask.ToString()).Id;
            int typeDefectId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.Defect.ToString()).Id;
            int typeChangeRequestId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.ChangeRequest.ToString()).Id;
            int typeEnhancementId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.Enhancement.ToString()).Id;
            int typeGeneralTaskId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.GeneralTask.ToString()).Id;

            // Get ticket priority Ids
            int priorityLowId = context.TicketPriorities.FirstOrDefault(t => t.Name == TicketPriorityEnum.Low.ToString()).Id;
            int priorityMediumId = context.TicketPriorities.FirstOrDefault(t => t.Name == TicketPriorityEnum.Medium.ToString()).Id;
            int priorityHighId = context.TicketPriorities.FirstOrDefault(t => t.Name == TicketPriorityEnum.High.ToString()).Id;
            int priorityUrgentId = context.TicketPriorities.FirstOrDefault(t => t.Name == TicketPriorityEnum.Urgent.ToString()).Id;

            // Get ticket status Ids
            int statusNewId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.New.ToString()).Id;
            int statusDevelopmentId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.Development.ToString()).Id;
            int statusTestingId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.Testing.ToString()).Id;
            int statusClosedId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.Closed.ToString()).Id;

            try
            {
                IList<Ticket> tickets = new List<Ticket>
                {
                    // Portfolio tickets
                    new Ticket() {Title = "PortfolioTicket 1", Description = "Ticket details for portfolio ticket 1", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityLowId, TicketStatusId = statusNewId, TicketTypeId = typeNewId},
                    new Ticket() {Title = "PortfolioTicket 2", Description = "Ticket details for portfolio ticket 2", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityMediumId, TicketStatusId = statusDevelopmentId, TicketTypeId = typeDefectId},
                    new Ticket() {Title = "PortfolioTicket 3", Description = "Ticket details for portfolio ticket 3", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityHighId, TicketStatusId = statusClosedId, TicketTypeId = typeEnhancementId},
                    // Blog
                    new Ticket() {Title = "Blog Ticket 1", Description = "Ticket details for blog ticket 1", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityLowId, TicketStatusId = statusNewId, TicketTypeId = typeNewId},
                    new Ticket() {Title = "Blog Ticket 2", Description = "Ticket details for blog ticket 2", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityUrgentId, TicketStatusId = statusDevelopmentId, TicketTypeId = typeChangeRequestId},
                    // BugTracker
                    new Ticket() {Title = "DUST Ticket 1", Description = "Ticket details for DUST ticket 1", Created = DateTimeOffset.Now, ProjectId = bugTrackerId, TicketPriorityId = priorityMediumId, TicketStatusId = statusDevelopmentId, TicketTypeId = typeDefectId},
                    new Ticket() {Title = "DUST Ticket 2", Description = "Ticket details for DUST ticket 2", Created = DateTimeOffset.Now, ProjectId = bugTrackerId, TicketPriorityId = priorityUrgentId, TicketStatusId = statusClosedId, TicketTypeId = typeDefectId},
                    // AddressBook
                    new Ticket() {Title = "AddressBook ticket 1", Description = "Ticket details for AddressBook ticket 1", Created = DateTimeOffset.Now, ProjectId = addressBookId, TicketPriorityId = priorityLowId, TicketStatusId = statusNewId, TicketTypeId = typeNewId},
                    new Ticket() {Title = "AddressBook ticket 2", Description = "Ticket details for AddressBook ticket 2", Created = DateTimeOffset.Now, ProjectId = addressBookId, TicketPriorityId = priorityMediumId, TicketStatusId = statusDevelopmentId, TicketTypeId = typeDefectId},
                    new Ticket() {Title = "AddressBook ticket 3", Description = "Ticket details for AddressBook ticket 3", Created = DateTimeOffset.Now, ProjectId = addressBookId, TicketPriorityId = priorityHighId, TicketStatusId = statusClosedId, TicketTypeId = typeEnhancementId},
                    // Mortgage Calculator
                    new Ticket() {Title = "Mortage calculator Ticket 1", Description = "Ticket details for Mortage calculator ticket 1", Created = DateTimeOffset.Now, ProjectId = mortgageCalcId, TicketPriorityId = priorityLowId, TicketStatusId = statusNewId, TicketTypeId = typeNewId},
                    new Ticket() {Title = "Mortage calculator Ticket 2", Description = "Ticket details for Mortage calculator ticket 2", Created = DateTimeOffset.Now, ProjectId = mortgageCalcId, TicketPriorityId = priorityUrgentId, TicketStatusId = statusDevelopmentId, TicketTypeId = typeChangeRequestId}                    
                };

                List<string> existingTicketNames = context.Tickets.Select(t => t.Title).ToList();

                await context.Tickets.AddRangeAsync(tickets.Where(t => !existingTicketNames.Contains(t.Title)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**** ERROR ****");
                Debug.WriteLine("Error seeding tickets");
                Debug.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
