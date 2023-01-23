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
                    new Company() {Name = "Together Bank", Description = "At Together Bank, we make it easy for you to manage your hard-earned money. We keep it safe too!"},
                    new Company() {Name = "E-Commerce Inc.", Description = "Our site is the fastest, most secure, most fun to shop on the Internet."},
                    new Company() {Name = "My Board", Description = "This is where I keep track of my projects, track bugs, make notes of new features I want to add, etc."},
                };

                List<string> existingCompanyNames = context.Companies.Select(c => c.Name).ToList();
                await context.Companies.AddRangeAsync(defaultCompanies.Where(c => !existingCompanyNames.Contains(c.Name)));
                await context.SaveChangesAsync();

                company1Id = context.Companies.FirstOrDefault(c => c.Name == "Together Bank").Id;
                company2Id = context.Companies.FirstOrDefault(c => c.Name == "E-Commerce Inc.").Id;
                company3Id = context.Companies.FirstOrDefault(c => c.Name == "My Board").Id;
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

                    /* Company 1: Together Bank */
                    new Project()
                    {
                        CompanyId = company1Id,
                        Name = "Cross-platform spending forecast",
                        Description = "Use data gathered from users to predict future spending and minimize credit defaults",
                        StartDate = new DateTime(2021,10,27),
                        EndDate = new DateTime(2022,07,30),
                        ProjectPriorityId = high.Id
                    },
                    new Project()
                    {
                        CompanyId = company1Id,
                        Name = "Update back-end systems to Blazor Server",
                        Description = "Although robust, our back-end system is slow and it is not ready for future cloud native solutions. ",
                        StartDate = new DateTime(2022,01,01),
                        EndDate = new DateTime(2026,02,26),
                        ProjectPriorityId = medium.Id
                    },
                    new Project()
                    {
                        CompanyId = company1Id,
                        Name = "Implement Apple Pay",
                        Description = "The most requested feature by our users. It will allow them to checkout faster both in-store and online.",
                        StartDate = new DateTime(2022,02,01),
                        EndDate = new DateTime(2023,02,26),
                        ProjectPriorityId = medium.Id
                    },

                    /* Company 2: E-Commerce Inc. */

                    new Project()
                    {
                        CompanyId = company2Id,
                        Name = "Update our development environment to JDK 19",
                        Description = "Start testing virtual threads. This will hopefully allow us to scale back on reactive programming, " +
                                      "resulting in easier to write, maintain and debug code. ",
                        StartDate = new DateTime(2023,03,01),
                        EndDate = new DateTime(2024,03,15),
                        ProjectPriorityId = low.Id
                    },
                    new Project()
                    {
                        CompanyId = company2Id,
                        Name = "Allow users to share their shopping cart items",
                        Description = "Make it easier for users that want to share a list of items with someone else.",
                        StartDate = new DateTime(2023,03,01),
                        EndDate = new DateTime(2023,08,15),
                        ProjectPriorityId = medium.Id
                    },

                    /* Company 3: My Board */
                    new Project()
                    {
                        CompanyId = company3Id,
                        Name = "Build a Bug Tracker Web App",
                        Description = "Build a multi tennent,.NET Core MVC web app that allows users and clients to" +
                                      " track bugs and features. Implement identity and user roles.",
                        StartDate = new DateTime(2022,07,01),
                        EndDate = new DateTime(2023,02,20),
                        ProjectPriorityId = urgent.Id
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
            var globalPassword = Environment.GetEnvironmentVariable("GLOBAL_PASSWORD") ?? configuration["GlobalPassword"];

            #region Company 1: Together Bank

            #region Admin-Demo

            var admin1Email = Environment.GetEnvironmentVariable("ADMIN1_EMAIL") ?? configuration["ADMIN1_EMAIL"];

            var defaultAdmin1 = new DUSTUser
            {
                UserName = admin1Email,
                Email = admin1Email,
                FirstName = "Ermin",
                LastName = "Cranke",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(defaultAdmin1.Email);
                if (existingUser == null)
                {
                    var result = await userManager.CreateAsync(defaultAdmin1, globalPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(defaultAdmin1, RolesEnum.Admin.ToString());
                        await userManager.AddToRoleAsync(defaultAdmin1, RolesEnum.DemoUser.ToString());
                    } else
                    {
                        foreach (var error in result.Errors)
                        {
                           var description = error.Description;
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default admin user ***");
                Debug.WriteLine(ex.Message);
                throw;
            }
            #endregion

            #region Project Managers

            /* Project Manager 1 */

            var pm1_1email = Environment.GetEnvironmentVariable("PM1_1_EMAIL") ?? configuration["PM1_1_EMAIL"];

            var pmUser1_1 = new DUSTUser
            {
                UserName = pm1_1email,
                Email = pm1_1email,
                FirstName = "Jeth",
                LastName = "Pierse",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(pmUser1_1.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(pmUser1_1, globalPassword);
                    await userManager.AddToRoleAsync(pmUser1_1, RolesEnum.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default project manager ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Project Manager 2 */

            var pm1_2email = Environment.GetEnvironmentVariable("PM1_2_EMAIL") ?? configuration["PM1_2_EMAIL"];

            var pmUser1_2 = new DUSTUser
            {
                UserName = pm1_2email,
                Email = pm1_2email,
                FirstName = "Gaile",
                LastName = "Petofi",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(pmUser1_2.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(pmUser1_2, globalPassword);
                    await userManager.AddToRoleAsync(pmUser1_2, RolesEnum.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default project manager ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Project Manager 3 */

            var pm1_3email = Environment.GetEnvironmentVariable("PM1_3_EMAIL") ?? configuration["PM1_3_EMAIL"];

            var pmUser1_3 = new DUSTUser
            {
                UserName = pm1_3email,
                Email = pm1_3email,
                FirstName = "Caria",
                LastName = "Strodder",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(pmUser1_3.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(pmUser1_3, globalPassword);
                    await userManager.AddToRoleAsync(pmUser1_3, RolesEnum.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default project manager ***");
                Debug.WriteLine(ex.Message);
                throw;
            }
            #endregion

            #region Developers, Demo

            /* Developer 1 - Demo */
            var dev1_1email = Environment.GetEnvironmentVariable("DEV1_1_EMAIL") ?? configuration["DEV1_1_EMAIL"];

            var devUser1_1 = new DUSTUser
            {
                UserName = dev1_1email,
                Email = dev1_1email,
                FirstName = "Ham",
                LastName = "Stegers",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(devUser1_1.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(devUser1_1, globalPassword);
                    await userManager.AddToRoleAsync(devUser1_1, RolesEnum.Developer.ToString());
                    await userManager.AddToRoleAsync(devUser1_1, RolesEnum.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer 2 */
            var dev1_2email = Environment.GetEnvironmentVariable("DEV1_2_EMAIL") ?? configuration["DEV1_2_EMAIL"];

            var devUser1_2 = new DUSTUser
            {
                UserName = dev1_2email,
                Email = dev1_2email,
                FirstName = "Sandro",
                LastName = "Suarez",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(devUser1_2.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(devUser1_2, globalPassword);
                    await userManager.AddToRoleAsync(devUser1_2, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer 3 */
            var dev1_3email = Environment.GetEnvironmentVariable("DEV1_3_EMAIL") ?? configuration["DEV1_3_EMAIL"];

            var devUser1_3 = new DUSTUser
            {
                UserName = dev1_3email,
                Email = dev1_3email,
                FirstName = "Eric",
                LastName = "Parmiter",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(devUser1_3.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(devUser1_3, globalPassword);
                    await userManager.AddToRoleAsync(devUser1_3, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer 4 */
            var dev1_4email = Environment.GetEnvironmentVariable("DEV1_4_EMAIL") ?? configuration["DEV1_4_EMAIL"];

            var devUser1_4 = new DUSTUser
            {
                UserName = dev1_4email,
                Email = dev1_4email,
                FirstName = "Meng",
                LastName = "Cunegonde",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(devUser1_4.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(devUser1_4, globalPassword);
                    await userManager.AddToRoleAsync(devUser1_4, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer 5 */
            var dev1_5email = Environment.GetEnvironmentVariable("DEV1_5_EMAIL") ?? configuration["DEV1_5_EMAIL"];

            var devUser1_5 = new DUSTUser
            {
                UserName = dev1_5email,
                Email = dev1_5email,
                FirstName = "Bjorn",
                LastName = "Owlner",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(devUser1_5.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(devUser1_5, globalPassword);
                    await userManager.AddToRoleAsync(devUser1_5, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            #endregion

            #region Submitters

            /* Submitter 1 */
            var sub1_1email = Environment.GetEnvironmentVariable("SUB1_1_EMAIL") ?? configuration["SUB1_1_EMAIL"];

            var subUser1_1 = new DUSTUser
            {
                UserName = sub1_1email,
                Email = sub1_1email,
                FirstName = "Marie",
                LastName = "Saunier",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(subUser1_1.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(subUser1_1, globalPassword);
                    await userManager.AddToRoleAsync(subUser1_1, RolesEnum.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default submitter ***");
                Debug.WriteLine(ex.Message);
                throw;
            }


            #endregion

            #endregion

            #region Company 2: E-Commerce Inc.

            #region Admin

            var admin2Email = Environment.GetEnvironmentVariable("ADMIN2_EMAIL") ?? configuration["ADMIN2_EMAIL"];

            var defaultAdmin2 = new DUSTUser
            {
                UserName = admin2Email,
                Email = admin2Email,
                FirstName = "Worth",
                LastName = "Buff",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(defaultAdmin2.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(defaultAdmin2, globalPassword);
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
            #endregion

            #region Project Managers, Demo

            /* Project Manager 1 - Demo */

            var pm2_1email = Environment.GetEnvironmentVariable("PM2_1_EMAIL") ?? configuration["PM2_1_EMAIL"];

            var pmUser2_1 = new DUSTUser
            {
                UserName = pm2_1email,
                Email = pm2_1email,
                FirstName = "Kaile",
                LastName = "Luck",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(pmUser2_1.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(pmUser2_1, globalPassword);
                    await userManager.AddToRoleAsync(pmUser2_1, RolesEnum.ProjectManager.ToString());
                    await userManager.AddToRoleAsync(pmUser2_1, RolesEnum.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default project manager ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Project Manager 2 */

            var pm2_2email = Environment.GetEnvironmentVariable("PM2_2_EMAIL") ?? configuration["PM2_2_EMAIL"];

            var pmUser2_2 = new DUSTUser
            {
                UserName = pm2_2email,
                Email = pm2_2email,
                FirstName = "Roanna",
                LastName = "Upstell",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(pmUser2_2.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(pmUser2_2, globalPassword);
                    await userManager.AddToRoleAsync(pmUser2_2, RolesEnum.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default project manager ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            #endregion

            #region Developers

            /* Developer 1 */
            var dev2_1email = Environment.GetEnvironmentVariable("DEV2_1_EMAIL") ?? configuration["DEV2_1_EMAIL"];

            var devUser2_1 = new DUSTUser
            {
                UserName = dev2_1email,
                Email = dev2_1email,
                FirstName = "Melia",
                LastName = "Bartol",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(devUser2_1.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(devUser2_1, globalPassword);
                    await userManager.AddToRoleAsync(devUser2_1, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer 2 */
            var dev2_2email = Environment.GetEnvironmentVariable("DEV2_2_EMAIL") ?? configuration["DEV2_2_EMAIL"];

            var devUser2_2 = new DUSTUser
            {
                UserName = dev1_2email,
                Email = dev1_2email,
                FirstName = "Gillian",
                LastName = "Vassay",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(devUser2_2.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(devUser2_2, globalPassword);
                    await userManager.AddToRoleAsync(devUser2_2, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            /* Developer 3 */
            var dev2_3email = Environment.GetEnvironmentVariable("DEV2_3_EMAIL") ?? configuration["DEV2_3_EMAIL"];

            var devUser2_3 = new DUSTUser
            {
                UserName = dev2_3email,
                Email = dev2_3email,
                FirstName = "Torin",
                LastName = "O'Gormally",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(devUser2_3.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(devUser2_3, globalPassword);
                    await userManager.AddToRoleAsync(devUser2_3, RolesEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default developer ***");
                Debug.WriteLine(ex.Message);
                throw;
            }

            #endregion

            #region Submitters

            /* Submitter 1 */
            var sub2_1email = Environment.GetEnvironmentVariable("SUB2_1_EMAIL") ?? configuration["SUB2_1_EMAIL"];

            var subUser2_1 = new DUSTUser
            {
                UserName = sub2_1email,
                Email = sub2_1email,
                FirstName = "Devan",
                LastName = "Lowson",
                EmailConfirmed = true,
                CompanyId = company2Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(subUser2_1.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(subUser2_1, globalPassword);
                    await userManager.AddToRoleAsync(subUser2_1, RolesEnum.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default submitter ***");
                Debug.WriteLine(ex.Message);
                throw;
            }


            #endregion

            #endregion

            #region Company 3: My Board

            #region Admin

            var myEmail = Environment.GetEnvironmentVariable("ADMIN_ME_EMAIL") ?? configuration["ADMIN_ME:EMAIL"];
            var myPassword = Environment.GetEnvironmentVariable("ADMIN_ME_PASSWORD") ?? configuration["ADMIN_ME:PASSWORD"];

            var superAdmin = new DUSTUser
            {
                UserName = myEmail,
                Email = myEmail,
                FirstName = "Angel",
                LastName = "M",
                EmailConfirmed = true,
                CompanyId = company3Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(superAdmin.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(superAdmin, myPassword);
                    await userManager.AddToRoleAsync(superAdmin, RolesEnum.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default admin user ***");
                Debug.WriteLine(ex.Message);
                throw;
            }
            #endregion

            #region Submitter - Demo

            /* Submitter 1 */
            var sub3_1email = Environment.GetEnvironmentVariable("SUB3_1_EMAIL") ?? configuration["SUB3_1_EMAIL"];

            var subUser3_1 = new DUSTUser
            {
                UserName = sub3_1email,
                Email = sub3_1email,
                FirstName = "Breena",
                LastName = "Wisniewski",
                EmailConfirmed = true,
                CompanyId = company3Id
            };

            try
            {
                var existingUser = await userManager.FindByEmailAsync(subUser3_1.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(subUser3_1, globalPassword);
                    await userManager.AddToRoleAsync(subUser3_1, RolesEnum.Submitter.ToString());
                    await userManager.AddToRoleAsync(subUser3_1, RolesEnum.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** ERROR ***");
                Debug.WriteLine("*** Error seeding default submitter ***");
                Debug.WriteLine(ex.Message);
                throw;
            }


            #endregion

            #endregion

        }

        public static async Task SeedDefaultTicketTypeAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketType> ticketTypes = new List<TicketType>()
                {
                    new TicketType() {Name = TicketTypeEnum.New_Feature.ToString()},
                    new TicketType() {Name = TicketTypeEnum.Task.ToString()},
                    new TicketType() {Name = TicketTypeEnum.Bug.ToString()},
                    new TicketType() {Name = TicketTypeEnum.Change_Request.ToString()},
                    new TicketType() {Name = TicketTypeEnum.Improvement.ToString()},
                    new TicketType() {Name = TicketTypeEnum.Test.ToString()},
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
                    new TicketStatus() { Name = TicketStatusEnum.Open.ToString() },
                    new TicketStatus() { Name = TicketStatusEnum.In_Progress.ToString() },
                    new TicketStatus() { Name = TicketStatusEnum.Testing.ToString() },
                    new TicketStatus() { Name = TicketStatusEnum.Retest.ToString() },
                    new TicketStatus() { Name = TicketStatusEnum.Fixed.ToString() },
                    new TicketStatus() { Name = TicketStatusEnum.Closed.ToString() },
                    new TicketStatus() { Name = TicketStatusEnum.Cancelled.ToString() }
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
            int project1_1 = context.Projects.FirstOrDefault(p => p.Name == "Cross-platform spending forecast").Id;
            int project1_2 = context.Projects.FirstOrDefault(p => p.Name == "Update back-end systems to Blazor Server").Id;
            int project1_3 = context.Projects.FirstOrDefault(p => p.Name == "Implement Apple Pay").Id;
            int project2_1 = context.Projects.FirstOrDefault(p => p.Name == "Update our development environment to JDK 19").Id;
            int project2_2 = context.Projects.FirstOrDefault(p => p.Name == "Allow users to share their shopping cart items").Id;
            int project3_1 = context.Projects.FirstOrDefault(p => p.Name == "Build a Bug Tracker Web App").Id;

            // Get ticket type Ids
            int type_NewFeatureId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.New_Feature.ToString()).Id;
            int type_WorkTaskId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.Task.ToString()).Id;
            int type_DefectId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.Bug.ToString()).Id;
            int type_ChangeRequestId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.Change_Request.ToString()).Id;
            int type_EnhancementId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.Improvement.ToString()).Id;
            int type_GeneralTaskId = context.TicketTypes.FirstOrDefault(t => t.Name == TicketTypeEnum.Test.ToString()).Id;

            // Get ticket priority Ids
            int priority_LowId = context.TicketPriorities.FirstOrDefault(t => t.Name == TicketPriorityEnum.Low.ToString()).Id;
            int priority_MediumId = context.TicketPriorities.FirstOrDefault(t => t.Name == TicketPriorityEnum.Medium.ToString()).Id;
            int priority_HighId = context.TicketPriorities.FirstOrDefault(t => t.Name == TicketPriorityEnum.High.ToString()).Id;
            int priority_UrgentId = context.TicketPriorities.FirstOrDefault(t => t.Name == TicketPriorityEnum.Urgent.ToString()).Id;

            // Get ticket status Ids
            int status_OpenId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.Open.ToString()).Id;
            int status_DevelopmentId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.In_Progress.ToString()).Id;
            int status_TestingId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.Testing.ToString()).Id;
            int status_RetestId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.Retest.ToString()).Id;
            int status_FixedId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.Fixed.ToString()).Id;
            int status_ClosedId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.Closed.ToString()).Id;
            int status_CancelledId = context.TicketStatuses.FirstOrDefault(t => t.Name == TicketStatusEnum.Cancelled.ToString()).Id;


            try
            {
                IList<Ticket> tickets = new List<Ticket>
                {
                    // Cross-platform spending forecast
                    new Ticket() {Title = "Login issue for mobile app users", Description = "Users are unable to log in to the bank's mobile app.", Created = DateTimeOffset.Now, ProjectId = project1_1, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_DefectId},
                    new Ticket() {Title = "Incorrect account balance displaying", Description = "Account balances are displaying incorrectly on the website and mobile app.", Created = DateTimeOffset.Now, ProjectId = project1_1, TicketPriorityId = priority_MediumId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_DefectId},
                    new Ticket() {Title = "Online banking outage", Description = "The bank's online banking platform is currently down and inaccessible to customers.", Created = DateTimeOffset.Now, ProjectId = project1_1, TicketPriorityId = priority_HighId, TicketStatusId = status_FixedId, TicketTypeId = type_WorkTaskId},

                    new Ticket() {Title = "Add Account Balance Notifications", Description = "Send notifications to customers when their account balance reaches a certain threshold for better account management.", Created = DateTimeOffset.Now, ProjectId = project1_1, TicketPriorityId = priority_HighId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_EnhancementId},
                    new Ticket() {Title = "Implement Cardless ATM Withdrawals", Description = "Allow customers to withdraw cash from ATMs using their mobile device for added convenience.", Created = DateTimeOffset.Now, ProjectId = project1_1, TicketPriorityId = priority_HighId, TicketStatusId = status_TestingId, TicketTypeId = type_EnhancementId},
                    new Ticket() {Title = "Implement Account Aggregation", Description = "Allow customers to view their accounts from other financial institutions in their online banking account for better financial overview.", Created = DateTimeOffset.Now, ProjectId = project1_1, TicketPriorityId = priority_HighId, TicketStatusId = status_ClosedId, TicketTypeId = type_NewFeatureId},

                    // Blazor
                    new Ticket() {Title = "Blazor Server Migration Error,", Description = "Fix an error that occurred during the migration of the backend to Blazor Server, this error is preventing the application from running correctly.", Created = DateTimeOffset.Now, ProjectId = project1_2, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_DefectId},
                    new Ticket() {Title = "Transaction errors on ATM machines", Description = "Customers are experiencing errors when trying to complete transactions on ATM machines", Created = DateTimeOffset.Now, ProjectId = project1_2, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_DefectId},
                    new Ticket() {Title = "Unable to set up new payees", Description = "Customers are unable to set up new payees on the online banking platform.", Created = DateTimeOffset.Now, ProjectId = project1_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_DefectId},
                    new Ticket() {Title = "Security breach notification", Description = "The bank has detected a potential security breach and is informing customers to take action.", Created = DateTimeOffset.Now, ProjectId = project1_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_WorkTaskId},
                    new Ticket() {Title = "Direct deposit issues", Description = "Customers are reporting issues with direct deposits not being credited to their accounts.", Created = DateTimeOffset.Now, ProjectId = project1_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_ChangeRequestId},

                    new Ticket() {Title = "Enhance Online Banking Dashboard", Description = "Improve the design and functionality of the online banking dashboard for better user experience and ease of use.", Created = DateTimeOffset.Now, ProjectId = project1_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_EnhancementId},
                    new Ticket() {Title = "Implement P2P Payment Feature", Description = "Allow customers to make peer-to-peer payments directly from their online banking account for added convenience.", Created = DateTimeOffset.Now, ProjectId = project1_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_EnhancementId},
                    new Ticket() {Title = "Blazor Server API Integration", Description = "As a developer, I want to integrate the API with Blazor Server to allow for dynamic updates to the front-end.", Created = DateTimeOffset.Now, ProjectId = project1_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_NewFeatureId},
                    new Ticket() {Title = "Blazor Server Security Implementation", Description = "As a developer, I want to implement security measures for the Blazor Server backend to ensure the protection of sensitive user data.", Created = DateTimeOffset.Now, ProjectId = project1_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_NewFeatureId},

                    // Apple Pay
                    new Ticket() {Title = "Duplicate charges on credit card accounts", Description = "Customers are reporting duplicate charges on their credit card accounts.", Created = DateTimeOffset.Now, ProjectId = project1_3, TicketPriorityId = priority_MediumId, TicketStatusId = status_RetestId, TicketTypeId = type_DefectId},
                    new Ticket() {Title = "Incorrect interest calculation", Description = "Interest is being calculated incorrectly on certain account types.", Created = DateTimeOffset.Now, ProjectId = project1_3, TicketPriorityId = priority_UrgentId, TicketStatusId = status_ClosedId, TicketTypeId = type_DefectId},

                    new Ticket() {Title = "Implement Touch ID Login", Description = "Allow customers to log in to the bank's mobile app using their fingerprint for added security.", Created = DateTimeOffset.Now, ProjectId = project1_3, TicketPriorityId = priority_UrgentId, TicketStatusId = status_TestingId, TicketTypeId = type_EnhancementId},
                    new Ticket() {Title = "Implement Personal Financial Management Feature", Description = "Provide customers with tools to manage their finances, such as creating a savings plan for better financial planning.", Created = DateTimeOffset.Now, ProjectId = project1_3, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_NewFeatureId},
                    new Ticket() {Title = "Apple Pay Integration", Description = "As a customer, I want to be able to use Apple Pay as a payment option during checkout process.", Created = DateTimeOffset.Now, ProjectId = project1_3, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_NewFeatureId},
                    new Ticket() {Title = "Apple Pay Error Handling", Description = "As a developer, I want to implement error handling for issues related to Apple Pay integration to ensure a smooth user experience.", Created = DateTimeOffset.Now, ProjectId = project1_3, TicketPriorityId = priority_UrgentId, TicketStatusId = status_CancelledId, TicketTypeId = type_ChangeRequestId},
                    
                    // JDK 19
                    new Ticket() {Title = "Java 19 Compatibility Issues with Libraries", Description = "Several external libraries used by the project are not compatible with Java 19 and need to be updated or replaced.", Created = DateTimeOffset.Now, ProjectId = project2_1, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_NewFeatureId},
                    new Ticket() {Title = "Java 19 Deprecation Warnings", Description = "Updating to Java 19 is causing deprecation warnings in the codebase that need to be addressed.", Created = DateTimeOffset.Now, ProjectId = project2_1, TicketPriorityId = priority_MediumId, TicketStatusId = status_CancelledId, TicketTypeId = type_DefectId},
                    new Ticket() {Title = "Java 19 Performance Regression", Description = "Performance tests have shown a regression in performance after updating to Java 19 and the cause needs to be investigated.", Created = DateTimeOffset.Now, ProjectId = project2_1, TicketPriorityId = priority_HighId, TicketStatusId = status_ClosedId, TicketTypeId = type_EnhancementId},
                    new Ticket() {Title = "Java 19 Unsupported Features", Description = "Certain features used in the codebase are not supported in Java 19 and need to be refactored or replaced with alternative solutions.", Created = DateTimeOffset.Now, ProjectId = project2_1, TicketPriorityId = priority_HighId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_EnhancementId},

                    // Shopping Cart 
                    new Ticket() {Title = "Checkout Page Error", Description = "Some customers are reporting an error message when trying to complete a purchase on the checkout page.", Created = DateTimeOffset.Now, ProjectId = project2_2, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_DefectId},
                    new Ticket() {Title = "Incorrect Product Pricing", Description = "Certain products are displaying the wrong price on the website.", Created = DateTimeOffset.Now, ProjectId = project2_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_DefectId},
                    new Ticket() {Title = "Shipping Address Validation Error", Description = "Customers are reporting that the website is not accepting valid shipping addresses", Created = DateTimeOffset.Now, ProjectId = project2_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_DefectId},
                    new Ticket() {Title = "Duplicate Order Confirmation Emails", Description = "Customers are receiving multiple order confirmation emails for a single purchase.", Created = DateTimeOffset.Now, ProjectId = project2_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_GeneralTaskId},
                    new Ticket() {Title = "Slow Loading Product Pages", Description = "Certain product pages are taking a long time to load, causing a poor user experience.", Created = DateTimeOffset.Now, ProjectId = project2_2, TicketPriorityId = priority_UrgentId, TicketStatusId = status_DevelopmentId, TicketTypeId = type_ChangeRequestId},

                    new Ticket() {Title = "Wishlist Feature", Description = "As a customer, I want to be able to save items to a wishlist so that I can easily find them later and purchase them.", Created = DateTimeOffset.Now, ProjectId = project2_2, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_NewFeatureId},
                    new Ticket() {Title = "Gift Card Support", Description = "As a customer, I want to be able to purchase and redeem gift cards to use on the website.", Created = DateTimeOffset.Now, ProjectId = project2_2, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_NewFeatureId},
                    new Ticket() {Title = "Subscription-based Purchases", Description = "As a customer, I want to be able to sign up for a recurring subscription for certain products.", Created = DateTimeOffset.Now, ProjectId = project2_2, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_NewFeatureId},
                    new Ticket() {Title = "Order Tracking", Description = "As a customer, I want to be able to track the status of my order and receive updates on its delivery.", Created = DateTimeOffset.Now, ProjectId = project2_2, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_NewFeatureId},
                    new Ticket() {Title = "Saved Payment Methods", Description = "As a customer, I want to be able to save my payment method information for faster checkout in the future.", Created = DateTimeOffset.Now, ProjectId = project2_2, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_NewFeatureId},


                    // BugTracker
                    new Ticket() {Title = "Implement Demo Buttons", Description = "As a new user, I want see what the app has to offer without creating an account, to save time and avoid exposing sensitive data", Created = DateTimeOffset.Now, ProjectId = project3_1, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_NewFeatureId},
                    new Ticket() {Title = "Implement Notifications", Description = "Implement notifications so that users can quickly see when a change is made in the project or in a ticket, or if there's a new message", Created = DateTimeOffset.Now, ProjectId = project3_1, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_NewFeatureId},
                    new Ticket() {Title = "Error when clicking 'Details' in 'AllTickets', logged in as a developer", Description = "Getting object reference not set.", Created = DateTimeOffset.Now, ProjectId = project3_1, TicketPriorityId = priority_LowId, TicketStatusId = status_OpenId, TicketTypeId = type_DefectId},
                
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
