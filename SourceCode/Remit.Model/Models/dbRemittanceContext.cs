using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Transcend.Model.Models.Mapping;

namespace Transcend.Model.Models
{
    public partial class dbRemittanceContext : DbContext
    {
        static dbRemittanceContext()
        {
            Database.SetInitializer<dbRemittanceContext>(null);
        }

        public dbRemittanceContext()
            : base("Name=dbRemittanceContext")
        {
        }

        public DbSet<ActionLog> ActionLogs { get; set; }
        public DbSet<Api> Apis { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<BusinessUser> BusinessUsers { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<ExchangeHouse> ExchangeHouses { get; set; }
        public DbSet<ExHApi> ExHApis { get; set; }
        public DbSet<ExHIPAddress> ExHIPAddresses { get; set; }
        public DbSet<ExHRemitData> ExHRemitDatas { get; set; }
        public DbSet<ExHRemitDataDetail> ExHRemitDataDetails { get; set; }
        public DbSet<ExHUser> ExHUsers { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleSubModuleItem> RoleSubModuleItems { get; set; }
        public DbSet<SubModule> SubModules { get; set; }
        public DbSet<SubModuleItem> SubModuleItems { get; set; }
        public DbSet<Workflowaction> Workflowactions { get; set; }
        public DbSet<WorkflowactionSetting> WorkflowactionSettings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ActionLogMap());
            modelBuilder.Configurations.Add(new ApiMap());
            modelBuilder.Configurations.Add(new BranchMap());
            modelBuilder.Configurations.Add(new BusinessUserMap());
            modelBuilder.Configurations.Add(new CityMap());
            modelBuilder.Configurations.Add(new CompanyMap());
            modelBuilder.Configurations.Add(new CountryMap());
            modelBuilder.Configurations.Add(new CurrencyMap());
            modelBuilder.Configurations.Add(new DepartmentMap());
            modelBuilder.Configurations.Add(new DesignationMap());
            modelBuilder.Configurations.Add(new EmployeeMap());
            modelBuilder.Configurations.Add(new ExchangeHouseMap());
            modelBuilder.Configurations.Add(new ExHApiMap());
            modelBuilder.Configurations.Add(new ExHIPAddressMap());
            modelBuilder.Configurations.Add(new ExHRemitDataMap());
            modelBuilder.Configurations.Add(new ExHRemitDataDetailMap());
            modelBuilder.Configurations.Add(new ExHUserMap());
            modelBuilder.Configurations.Add(new ModuleMap());
            modelBuilder.Configurations.Add(new RoleMap());
            modelBuilder.Configurations.Add(new RoleSubModuleItemMap());
            modelBuilder.Configurations.Add(new SubModuleMap());
            modelBuilder.Configurations.Add(new SubModuleItemMap());
            modelBuilder.Configurations.Add(new WorkflowactionMap());
            modelBuilder.Configurations.Add(new WorkflowactionSettingMap());
        }
    }
}
