using System.Collections.Immutable;
using System.Linq.Expressions;
using BackEndAPI.Models;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.Approval_V2;
using BackEndAPI.Models.Approval;
using BackEndAPI.Models.ARInvoice;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.Branchs;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Committed;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Fee;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.SaleForecastModel;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Models.StorageFee;
using BackEndAPI.Models.TaxGroup;
using BackEndAPI.Models.Unit;
using BackEndAPI.Service.Notification;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static BackEndAPI.Models.Reports.InventoryReport;
using BackEndAPI.Models.PriceList;
using BackEndAPI.Models.ConfirmationSystem;

namespace BackEndAPI.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int,
        AppUserClaim, AppUserRole, AppUserLogin,
        AppRoleClaim, AppUserToken>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Sync.SyncCheckpoint> SyncCheckpoints { get; set; }
        public DbSet<Models.Account.RefreshToken> RefreshTokens { get; set; }
        public DbSet<ConfirmationDocument> ConfirmationDocuments { get; set; }
        public DbSet<ConfirmationLog> ConfirmationLogs { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<RatingImage> RatingImages { get; set; }
        public DbSet<Models.DebtReconciliation> DebtReconciliations { get; set; }
        public DbSet<PaymentInfo> PaymentInfo { get; set; }
        public DbSet<Models.DebtReconciliationAttachment> DebtReconciliationAttachments { get; set; }

        public DbSet<Models.ItemMasterData.ProductApplications> ProductApplications { get; set; }
        public DbSet<Models.ItemMasterData.ProductGroup> ProductGroup { get; set; }
        public DbSet<Models.ItemMasterData.ProductQualityLevel> ProductQualityLevel { get; set; }
        public DbSet<PaymentRule> PaymentRule { get; set; }
        public DbSet<FeePeriod> FeePeriod { get; set; }
        public DbSet<FeebyCustomer> FeebyCustomer { get; set; }
        public DbSet<BP> BP { get; set; }
        public DbSet<ZaloAccess> ZaloAccess { get; set; }
        public DbSet<TaxGroups> TaxGroups { get; set; }
        public DbSet<CRD1> CRD1 { get; set; }
        public DbSet<CRD2> CRD2 { get; set; }
        public DbSet<CRD3> CRD3 { get; set; }
        public DbSet<CRD4> CRD4 { get; set; }
        public DbSet<CRD5> CRD5 { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<Redeem> Redeem { get; set; }

        public DbSet<RedeemLine> RedeemLines { get; set; }

        //public DbSet<CustomerPointHistory> CustomerPointHistories { get; set; }
        public DbSet<CustomerPoint> CustomerPoint { get; set; }
        public DbSet<CustomerPointLine> CustomerPointLine { get; set; }
        public DbSet<CustomerPointCycle> CustomerPointCycles { get; set; }
        public DbSet<PointSetup> PointSetups { get; set; }
        public DbSet<PointSetupCustomer> PointSetupCustomers { get; set; }
        public DbSet<PointSetupLine> PointSetupLines { get; set; }
        public DbSet<PointSetupIndustry> PointSetupIndustries { get; set; }
        public DbSet<PointSetupBrand> PointSetupBrands { get; set; }
        public DbSet<PointSetupItemType> PointSetupItemTypes { get; set; }
        public DbSet<PointSetupPacking> PointSetupPackings { get; set; }
        public DbSet<ItemListView> ItemListViews { get; set; }
        public DbSet<Article> Article { get; set; }
        public DbSet<ITM1> ITM1 { get; set; }
        public DbSet<ItemType> ItemType { get; set; }
        public DbSet<Industry> Industry { get; set; }
        public DbSet<Brand> Brand { get; set; }
        public DbSet<Packing> Packing { get; set; }
        public DbSet<OIBT> OIBT { get; set; }
        public DbSet<BPArea> BPArea { get; set; }
        public DbSet<BPSize> BPSize { get; set; }
        public DbSet<OCRG> OCRG { get; set; }
        public DbSet<ODOC> ODOC { get; set; }
        public DbSet<DOC1> DOC1 { get; set; }
        public DbSet<DOC2> DOC2 { get; set; }
        public DbSet<DOC12> DOC12 { get; set; }
        public DbSet<DOC16> DOC16 { get; set; }
        public DbSet<DOC14> DOC14 { get; set; }
        public DbSet<OUOM> OUOM { get; set; }
        public DbSet<OUGP> OUGP { get; set; }
        public DbSet<UGP1> UGP1 { get; set; }

        public DbSet<Promotion> Promotion { get; set; }

        public DbSet<ExchangePoint> ExchangePoint { get; set; }
        public DbSet<ExchangePointLine> ExchangePointLine { get; set; }
        public DbSet<PointCustomer> PointCustomer { get; set; }
        public DbSet<PromotionLine> PromotionLine { get; set; }
        public DbSet<PromotionCustomer> PromotionCustomer { get; set; }
        public DbSet<PromotionBrand> PromotionBrand { get; set; }
        public DbSet<PromotionIndustry> PromotionIndustry { get; set; }
        public DbSet<PromotionLineSub> PromotionLineSub { get; set; }
        public DbSet<PromotionLineSubSub> PromotionLineSubSub { get; set; }
        public DbSet<PromotionItemBuy> PromotionItemBuy { get; set; }
        public DbSet<PromotionUnit> PromotionUnit { get; set; }

        public DbSet<PromotionSubItemAdd> PromotionSubItemAdd { get; set; }
        public DbSet<AttDocument> AttDocument { get; set; }
        public DbSet<PromotionSubUnit> PromotionSubUnit { get; set; }
        public DbSet<ItemSpec> ItemSpec { get; set; }
        public DbSet<Voucher> Voucher { get; set; }
        public DbSet<VoucherItem> VoucherItem { get; set; }
        public DbSet<VoucherLine> VoucherLine { get; set; }
        public DbSet<VoucherSystem> VoucherSystem { get; set; }
        public DbSet<VoucherCustomer> VoucherCustomer { get; set; }
        public DbSet<VoucherSeller> VoucherSeller { get; set; }
        public DbSet<Coupon> Coupon { get; set; }
        public DbSet<CouponItem> CouponItem { get; set; }
        public DbSet<CouponLine> CouponLine { get; set; }
        public DbSet<PaymentMethod> PaymentMethod { get; set; }
        public DbSet<PersonInfor> PersonInfor { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<TimeZones> TimeZones { get; set; }
        public DbSet<OWST> OWST { get; set; }
        public DbSet<WST1> WST1 { get; set; }
        public DbSet<OWDD> OWDD { get; set; }
        public DbSet<WDD1> WDD1 { get; set; }
        public DbSet<AreaLocation> AreaLocation { get; set; }
        public DbSet<Branch> Branch { get; set; }
        public DbSet<BranchAddress> BranchAddress { get; set; }
        public DbSet<Privileges> Privileges { get; set; }
        public DbSet<AppUser> AppUser { get; set; }
        public DbSet<AppUserClaim> AppUserClaim { get; set; }
        public DbSet<AppRole> AppRole { get; set; }
        public DbSet<AppUserRole> AppUserRole { get; set; }
        public DbSet<AppRoleClaim> AppRoleClaim { get; set; }

        public DbSet<AppSetting> AppSetting { get; set; }

        public DbSet<OWTM> OWTM { get; set; }
        public DbSet<WTM1> WTM1 { get; set; }
        public DbSet<WTM2> WTM2 { get; set; }
        public DbSet<WTM3> WTM3 { get; set; }
        public DbSet<WTM4> WTM4 { get; set; }
        public DbSet<Fee> Fee { get; set; }
        public DbSet<FeeLine> FeeLine { get; set; }
        public DbSet<FeeLevel> FeeLevel { get; set; }


        public DbSet<Committed> Committed { get; set; }
        public DbSet<CommittedLine> CommittedLine { get; set; }
        public DbSet<CommittedLineSub> CommittedLineSub { get; set; }

        public DbSet<CommittedLineSubSub> CommittedLineSubSub { get; set; }
        public DbSet<CommittedTracking> CommittedTracking { get; set; }
        public DbSet<CommittedTrackingLine> CommittedTrackingLine { get; set; }
        public DbSet<Models.OrganizationUnit> OrganizationUnit { get; set; }

        // Jarviisha
        // public DbSet<Models.StorageFee.StorageFeeZZ> StorageFees { get; set; }
        public DbSet<Models.StorageFee.StorageFee> StorageFees { get; set; }
        public DbSet<Models.StorageFee.FeeMilestone> FeeMilestones { get; set; }
        public DbSet<Models.StorageFee.StorageFeeLine> StorageFeeLines { get; set; }

        public DbSet<Models.Approval.Approval> Approval { get; set; }
        public DbSet<Models.Approval.ApprovalLine> ApprovalLine { get; set; }
        public DbSet<Models.NotificationModels.Notification> Notifications { get; set; }
        public DbSet<Models.NotificationModels.NotificationObject> NotificationObjects { get; set; }
        public DbSet<Models.NotificationModels.NotificationUser> NotificationUsers { get; set; }
        public DbSet<Models.Account.UserGroup> UserGroup { get; set; }
        public DbSet<Models.ProductionCommitmentModel.ProductionCommitment> ProductionCommitment { get; set; }
        public DbSet<Models.ProductionCommitmentModel.CommitmentLine> CommitmentLines { get; set; }
        public DbSet<Models.ProductionCommitmentModel.CommitmentLineItem> CommitmentLineItems { get; set; }
        public DbSet<Models.ProductionCommitmentModel.CommitmentItemAttribute> CommitmentItemAttributes { get; set; }
        public DbSet<Models.ProductionCommitmentModel.DiscountCommitment> DiscountCommitments { get; set; }
        public DbSet<Models.SaleForecastModel.SaleForecast> SaleForecasts { get; set; }
        public DbSet<Models.SaleForecastModel.SaleForecastItem> SaleForecastItems { get; set; }
        public DbSet<Models.SaleForecastModel.SaleForecastItemPeriod> SaleForecastItemPeriods { get; set; }

        public DbSet<Models.BPGroups.ConditionCusGroup> ConditionCusGroups { get; set; }
        public DbSet<Models.BPGroups.ConditionCusGroupValue> ConditionCusGroupValues { get; set; }

        public DbSet<Models.ItemGroups.ConditionItemGroup> ConditionItemGroup { get; set; }
        public DbSet<Models.ItemGroups.ConditionItemGroupValue> ConditionItemGroupValue { get; set; }
        public DbSet<Models.SystemLog> SystemLogs { get; set; }

        public DbSet<Models.LocationModels.Area> Areas { get; set; }
        public DbSet<Models.LocationModels.Region> Regions { get; set; }
        public DbSet<Models.BusinessPartners.CRD6> CRD6 { get; set; }
        public DbSet<Models.Cart.Cart> Carts { get; set; }
        public DbSet<Models.Cart.CartItem> CartItems { get; set; }
        public DbSet<Models.PolicyOrderLog> PolicyOrderLogs { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<PriceListLine> PriceListLines { get; set; }
        public DbSet<ZaloTokens> ZaloTokens { get; set; }

        public DbSet<ApprovalLevel> ApprovalLevels { get; set; }
        public DbSet<ApprovalLevelLine> ApprovalLevelLines { get; set; }

        public DbSet<ApprovalSample> ApprovalSamples { get; set; }
        public DbSet<ApprovalSampleDocumentsLine> ApprovalSampleDocumentsLines { get; set; }
        public DbSet<ApprovalSampleMembersLine> ApprovalSampleMembersLines { get; set; }
        public DbSet<ApprovalSampleProcessesLine> ApprovalSampleProcessesLines { get; set; }

        public DbSet<ApprovalWorkFlow> ApprovalWorkFlows { get; set; }
        public DbSet<ApprovalWorkFlowLine> ApprovalWorkFlowLines { get; set; }
        public DbSet<ApprovalWorkFlowDocumentLine> ApprovalWorkFlowDocumentLines { get; set; }
        public DbSet<VnpayLogging> VnpayLogging { get; set; }


        // public DbSet<RoleFillCustomerGroup> RoleFillCustomerGroups { get; set; }


        // public IQueryable<ODOC> GetFilteredODOC(Models.Document.ODOCFilter filter)
        // {
        //     var query = this.ODOC  
        // }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.Entity<ConfirmationLog>()
                .HasOne(l => l.Document)
                .WithMany(d => d.Logs)
                .HasForeignKey(l => l.DocumentId);
            modelBuilder.Entity<ConfirmationDocument>()
                .HasIndex(d => d.CardId);
            modelBuilder.Entity<AppRole>(a =>
            {
                a.HasMany(e => e.RoleFillCustomerGroups).WithOne().HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<RoleFillCustomerGroup>(a =>
            {
                a.HasOne(e => e.Ocrg).WithMany().HasForeignKey(e => e.CustomerGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
                a.HasKey(e => new { e.CustomerGroupId, e.RoleId });
            });
            modelBuilder.Entity<PriceList>()
                .HasMany(p => p.PriceListLine)
                .WithOne(l => l.PriceList)
                .HasForeignKey(l => l.FatherId);
            modelBuilder.Entity<AppUser>()
                .HasMany(x => x.DirectStaff)
                .WithMany()
                .UsingEntity(t => t.ToTable("UserLinkStaff"));

            modelBuilder.Entity<Models.DebtReconciliation>(ent =>
            {
                ent.HasIndex(x => x.Code).IsUnique(true);
                ent.HasIndex(x => x.Name).IsUnique(true);
                ent.HasMany(x => x.Attachments).WithOne().HasForeignKey(x => x.DebtId).OnDelete(DeleteBehavior.Cascade);
                // ent.HasMany(x  => x.Attachments).WithOne().HasForeignKey(x => x.DebtId).OnDelete(DeleteBehavior.Cascade);
                ent.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
                ent.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
            });

            modelBuilder.Entity<Models.OrganizationUnit>(ent =>
            {
                ent.HasOne(p => p.ManagerUser).WithMany()
                    .HasForeignKey(t => t.ManagerUserId).IsRequired(false);

                ent.HasOne(p => p.Parent).WithMany()
                    .HasForeignKey(t => t.ParentId).IsRequired(false);

                ent.HasMany(p => p.Employees).WithOne()
                    .HasForeignKey(t => t.OrganizationId).IsRequired(false);

                ent.HasIndex(x => x.Code).IsUnique(true);
            });
            //modelBuilder.Entity<CustomerPointHistory>();
            modelBuilder.Entity<ZaloTokens>();
            modelBuilder.Entity<ZaloAccess>();
            modelBuilder.Entity<CustomerPointCycle>();
            modelBuilder.Entity<PaymentRule>();
            modelBuilder.Entity<AppSetting>();
            modelBuilder.Entity<Models.Cart.Cart>()
                .HasMany(b => b.Items)
                .WithOne()
                .HasForeignKey(f => f.CartId);

            modelBuilder.Entity<Models.Cart.Cart>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<Models.Cart.CartItem>()
                .HasOne(b => b.Item)
                .WithMany()
                .HasForeignKey(f => f.ItemId)
                .IsRequired(false);

            modelBuilder.Entity<Models.BusinessPartners.BP>()
                .HasOne(b => b.SaleStaff)
                .WithMany()
                .HasForeignKey(f => f.SaleId);

            // C0--
            modelBuilder.Entity<Models.ProductionCommitmentModel.ProductionCommitment>()
                .HasMany(p => p.CommitmentLines)
                .WithOne(p => p.ProductionCommitment)
                .HasForeignKey(p => p.CommitId);
            modelBuilder.Entity<Models.ProductionCommitmentModel.CommitmentLine>()
                .HasMany(p => p.Items)
                .WithOne(p => p.Line)
                .HasForeignKey(p => p.LineId);
            modelBuilder.Entity<Models.ProductionCommitmentModel.CommitmentLineItem>()
                .HasMany(p => p.DiscountCommitments)
                .WithOne(p => p.LineItem)
                .HasForeignKey(p => p.LineItemId);
            modelBuilder.Entity<Models.ProductionCommitmentModel.CommitmentLineItem>()
                .HasMany(p => p.CommitmentItemAttribute)
                .WithOne(p => p.LineItem)
                .HasForeignKey(p => p.LineItemId);
            // ---

            modelBuilder.Entity<StorageFee>().HasMany(p => p.Lines)
                .WithOne(pn => pn.StorageFee)
                .HasForeignKey(pn => pn.StorageFeeId);

            modelBuilder.Entity<Models.Account.UserGroup>().HasMany(p => p.ListUsers).WithMany(p => p.UserGroup)
                .UsingEntity(p => p.ToTable("m2m_UserGroup"));

            modelBuilder.Entity<Models.NotificationModels.Notification>().HasOne(u => u.Object)
                .WithOne(u => u.Notification)
                .HasForeignKey<Models.NotificationModels.NotificationObject>(u => u.NotificationId);

            modelBuilder.Entity<Models.NotificationModels.NotificationUser>()
                .HasOne(u => u.Notification)
                .WithMany(u => u.Users).HasForeignKey(p => p.NotificationId);


            modelBuilder.Entity<BP>().HasOne(u => u.UserInfo).WithOne(pn => pn.BPInfo)
                .HasForeignKey<AppUser>(b => b.CardId);

            modelBuilder.Entity<ApprovalLine>().HasOne(u => u.Approval).WithMany(p => p.Lines)
                .HasForeignKey(a => a.WddId);
            modelBuilder.Entity<ODOC>().HasMany(u => u.Approval).WithOne(u => u.Document)
                .HasForeignKey(u => u.DocId);

            modelBuilder.Entity<AppUser>().HasMany(u => u.ApprovalInfo)
                .WithOne(u => u.Actor).HasForeignKey(u => u.ActorId);

            modelBuilder.Entity<AppUser>().HasMany(u => u.ApprovalLine)
                .WithOne(u => u.User).HasForeignKey(u => u.UserId);
            modelBuilder.Entity<Approval>().HasOne(u => u.Owtm).WithMany(p => p.Approval).HasForeignKey(p => p.WtmId);

            modelBuilder.Entity<Models.BusinessPartners.CRD4>()
                .HasMany(p => p.Payments)
                .WithOne(p => p.Crd4)
                .HasForeignKey(p => p.DocCode);

            modelBuilder.Entity<Models.Approval.ApprovalLine>()
                .HasOne(p => p.Owst)
                .WithMany(p => p.ApprovalLines)
                .HasForeignKey(p => p.WstId);

            modelBuilder.Entity<StorageFeeLine>().HasOne(a => a.FeeMilestone).WithMany(pn => pn.StorageFeeLine)
                .HasForeignKey(p => p.FeeId);
            modelBuilder.Entity<ItemListView>().HasNoKey();
            modelBuilder.Entity<AppUser>().ToTable("Users");
            modelBuilder.Entity<AppUserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<AppRoleClaim>().ToTable("RoleClaims");
            modelBuilder.Entity<AppUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<AppUserToken>().ToTable("UserTokens");
            modelBuilder.Entity<AppUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<AppRole>().ToTable("Roles");
            modelBuilder.Entity<BP>(b => { b.HasMany(b => b.ODOC).WithOne(b => b.BP).HasForeignKey(b => b.CardId); });
            modelBuilder.Entity<AppUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne(e => e.User)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();
                b.HasOne(e => e.PersonRole)
                    .WithMany()
                    .HasForeignKey(b => b.PersonRoleId).IsRequired(false);

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne(e => e.User)
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
            modelBuilder.Entity<Article>().ToTable("Article");
            modelBuilder.Entity<AppRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });
            modelBuilder.Entity<Models.Account.AppRoleClaim>().HasOne(x => x.Privilege).WithMany()
                .HasForeignKey(p => p.PrivilegesId);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.ClrType.Namespace.StartsWith("Microsoft.AspNetCore.Identity"))
                {
                    entityType.SetTableName(entityType.GetTableName().Replace("AspNet", ""));
                }
            }

            modelBuilder.Entity<ItemSpec>();
            modelBuilder.Entity<OWST>();
            modelBuilder.Entity<OWST>()
                .HasMany(p => p.WST1)
                .WithOne(pi => pi.OWST)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<OWDD>();
            modelBuilder.Entity<OWDD>()
                .HasMany(p => p.WDD1)
                .WithOne(pi => pi.OWDD)
                .HasForeignKey(pi => pi.WddId);
            modelBuilder.Entity<OWST>()
                .HasIndex(g => g.Name)
                .IsUnique();

            modelBuilder.Entity<FeePeriod>();
            modelBuilder.Entity<FeePeriod>()
                .HasMany(p => p.FeePeriodLine)
                .WithOne(pi => pi.FeePeriod)
                .HasForeignKey(pi => pi.PeriodId);

            modelBuilder.Entity<FeebyCustomer>();
            modelBuilder.Entity<FeebyCustomer>()
                .HasMany(p => p.FeebyCustomerLine)
                .WithOne(pi => pi.FeebyCustomer)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<OWTM>();
            modelBuilder.Entity<OWTM>()
                .HasMany(p => p.WTM1)
                .WithOne(pi => pi.OWTM)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<WTM1>()
                .HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<OWTM>()
                .HasMany(p => p.WTM2)
                .WithOne(pi => pi.OWTM)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<WTM2>()
                .HasOne(u => u.OWST)
                .WithMany()
                .HasForeignKey(u => u.WtsId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OWTM>()
                .HasMany(p => p.WTM3)
                .WithOne(pi => pi.OWTM)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<OWTM>()
                .HasOne(p => p.WTM4)
                .WithOne(pi => pi.OWTM)
                .HasForeignKey<WTM4>(pi => pi.FatherId);

            modelBuilder.Entity<OWTM>()
                .HasIndex(g => g.Name)
                .IsUnique();

            modelBuilder.Entity<Fee>()
                .HasOne(u => u.appUser)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<OIBT>().ToTable("OIBT");
            modelBuilder.Entity<OCRG>().ToTable("OCRG");

            modelBuilder.Entity<Packing>().ToTable("Packing");
            modelBuilder.Entity<Packing>()
                .HasIndex(u => new { u.Code })
                .IsUnique();
            modelBuilder.Entity<Packing>()
                .HasIndex(u => new { u.Name })
                .IsUnique();
            modelBuilder.Entity<TaxGroups>().ToTable("TaxGroups");
            modelBuilder.Entity<TaxGroups>()
                .HasIndex(u => new { u.Code })
                .IsUnique();
            modelBuilder.Entity<TaxGroups>()
                .HasIndex(u => new { u.Name })
                .IsUnique();

            modelBuilder.Entity<Fee>()
                .HasMany(p => p.FeeLine)
                .WithOne(pi => pi.Fee)
                .HasForeignKey(pi => pi.FeeId);

            modelBuilder.Entity<FeeLine>()
                .HasOne(g => g.OUGP)
                .WithMany()
                .HasForeignKey(g => g.UgpId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FeeLine>()
                .HasIndex(u => new { u.FeeId, u.UgpId, u.FeeLevelId })
                .IsUnique();

            modelBuilder.Entity<FeeLevel>()
                .HasIndex(u => new { u.FromDays, u.ToDays })
                .IsUnique();

            modelBuilder.Entity<FeeLine>()
                .HasOne(g => g.FeeLevel)
                .WithMany()
                .HasForeignKey(g => g.FeeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OUGP>()
                .HasMany(p => p.UGP1)
                .WithOne(pi => pi.OUGP)
                .HasForeignKey(pi => pi.UgpId);

            modelBuilder.Entity<OUGP>()
                .HasIndex(g => g.UgpCode)
                .IsUnique();

            modelBuilder.Entity<OUGP>()
                .HasOne(g => g.OUOM)
                .WithMany()
                .HasForeignKey(g => g.BaseUom)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UGP1>()
                .HasOne(u => u.OUOM)
                .WithMany()
                .HasForeignKey(u => u.UomId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ItemType>().ToTable("ItemType");
            modelBuilder.Entity<ItemType>()
                .HasIndex(u => new { u.Code })
                .IsUnique();
            modelBuilder.Entity<ItemType>()
                .HasIndex(u => new { u.Name })
                .IsUnique();
            modelBuilder.Entity<Redeem>()
                .HasMany(h => h.Lines)
                .WithOne(l => l.Header)
                .HasForeignKey(l => l.HeaderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OIBT>(ent =>
            {
                ent.HasMany(x => x.ConditionItemGroups).WithOne().HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ConditionItemGroup>(ent =>
            {
                ent.HasMany(x => x.CondValues).WithOne().HasForeignKey(x => x.CondId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BPArea>().ToTable("BPArea");
            modelBuilder.Entity<BPArea>()
                .HasIndex(u => new { u.BPAreaName })
                .IsUnique();
            modelBuilder.Entity<BPSize>().ToTable("BPSize");
            modelBuilder.Entity<BPSize>()
                .HasIndex(u => new { u.Name })
                .IsUnique();
            modelBuilder.Entity<Brand>().ToTable("Brand");
            modelBuilder.Entity<Brand>()
                .HasIndex(u => new { u.Code })
                .IsUnique();
            modelBuilder.Entity<Brand>()
                .HasIndex(u => new { u.Name })
                .IsUnique();
            modelBuilder.Entity<Industry>().ToTable("Industry");
            modelBuilder.Entity<Industry>()
                .HasIndex(u => new { u.Code })
                .IsUnique();
            modelBuilder.Entity<Industry>()
                .HasIndex(u => new { u.Name })
                .IsUnique();

            modelBuilder.Entity<Item>().ToTable("OITM");
            modelBuilder.Entity<Item>().ToTable("OITM")
                .HasIndex(u => new { u.ItemCode })
                .IsUnique();

            modelBuilder.Entity<Item>().ToTable("OITM")
                .HasMany(p => p.ITM1)
                .WithOne(pi => pi.Item)
                .HasForeignKey(pi => pi.ItemId);

            modelBuilder.Entity<Item>().ToTable("OITM")
                .HasOne(p => p.ItemType)
                .WithMany()
                .HasForeignKey(g => g.ItemTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>().ToTable("OITM")
                .HasOne(p => p.Packing)
                .WithMany()
                .HasForeignKey(g => g.PackingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommittedTrackingLine>().ToTable("CommittedTrackingLine")
                .HasOne(p => p.CommittedLineSub)
                .WithMany()
                .HasForeignKey(g => g.CommittedLineSubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>().ToTable("OITM")
                .HasOne(p => p.Industry)
                .WithMany()
                .HasForeignKey(g => g.IndustryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>().ToTable("OITM")
                .HasOne(p => p.OUGP)
                .WithMany()
                .HasForeignKey(g => g.UgpEntry)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Item>().ToTable("OITM")
                .HasOne(p => p.Brand)
                .WithMany()
                .HasForeignKey(g => g.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>().ToTable("OITM")
                .HasOne(p => p.TaxGroups)
                .WithMany()
                .HasForeignKey(g => g.TaxGroupsId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BP>().ToTable("OCRD")
                .HasMany(p => p.CRD1)
                .WithOne(pi => pi.BP)
                .HasForeignKey(pi => pi.BPId);

            modelBuilder.Entity<ODOC>()
                .HasOne(p => p.PaymentInfo)
                .WithOne()
                .HasForeignKey<PaymentInfo>(pi => pi.DocId);
            modelBuilder.Entity<ODOC>().ToTable("ODOC")
                .HasMany(p => p.Tracking)
                .WithOne(pi => pi.Document)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<BP>().ToTable("OCRD")
                .HasMany(p => p.CRD5)
                .WithOne(pi => pi.BP)
                .HasForeignKey(pi => pi.BPId);

            modelBuilder.Entity<BP>().ToTable("OCRD")
                .HasMany(p => p.CRD2)
                .WithOne(pi => pi.BP)
                .HasForeignKey(pi => pi.BPId);

            modelBuilder.Entity<BP>().ToTable("OCRD")
                .HasMany(p => p.CRD3)
                .WithOne(pi => pi.BP)
                .HasForeignKey(pi => pi.BPId);

            modelBuilder.Entity<BP>().ToTable("OCRD")
                .HasMany(p => p.CRD4)
                .WithOne(pi => pi.BP)
                .HasForeignKey(pi => pi.BPId);

            modelBuilder.Entity<CustomerPoint>().ToTable("CustomerPoint")
                .HasMany(p => p.Details)
                .WithOne(pi => pi.CustomerPoint)
                .HasForeignKey(pi => pi.FatherId);
            modelBuilder.Entity<CustomerPoint>()
                .HasIndex(x => new { x.DocId, x.DocType, x.AddPoint })
                .IsUnique();


            modelBuilder.Entity<ODOC>().ToTable("ODOC")
                .HasMany(p => p.ItemDetail)
                .WithOne(pi => pi.Document)
                .HasForeignKey(pi => pi.FatherId);
            modelBuilder.Entity<ODOC>().ToTable("ODOC")
                .HasMany(p => p.Promotion)
                .WithOne(pi => pi.Document)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<ODOC>().ToTable("ODOC")
                .HasMany(p => p.Address)
                .WithOne()
                .HasForeignKey(p => p.FatherId);

            modelBuilder.Entity<ODOC>().ToTable("ODOC")
                .HasMany(p => p.PaymentMethod)
                .WithOne(pi => pi.Document)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<ODOC>().ToTable("ODOC")
                .HasMany(p => p.AttachFile)
                .WithOne(pi => pi.Document)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<OUOM>()
                .HasIndex(u => new { u.UomCode, u.UomName })
                .IsUnique();
            modelBuilder.Entity<Committed>()
                .HasMany(p => p.CommittedLine)
                .WithOne()
                .HasForeignKey(pi => pi.FatherId);
            modelBuilder.Entity<Committed>().HasIndex(g => g.CommittedCode).IsUnique();

            modelBuilder.Entity<CommittedLine>()
                .HasMany(c => c.CommittedLineSub)
                .WithOne()
                .HasForeignKey(gc => gc.FatherId);

            modelBuilder.Entity<CommittedLineSub>()
                .HasMany(c => c.CommittedLineSubSub)
                .WithOne()
                .HasForeignKey(gc => gc.FatherId);

            modelBuilder.Entity<CommittedLineSub>()
                .HasOne(p => p.Industry)
                .WithMany()
                .HasForeignKey(g => g.IndustryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Committed>().HasOne(p => p.BP).WithMany(b => b.Commiteds).HasForeignKey(p => p.CardId)
                .IsRequired(false);

            modelBuilder.Entity<CommittedTracking>()
                .HasMany(p => p.CommittedTrackingLine)
                .WithOne()
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<ExchangePoint>()
                .HasMany(p => p.ExchangePointLine)
                .WithOne(pi => pi.ExchangePoint)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<ExchangePoint>()
                .HasMany(p => p.PointCustomer)
                .WithOne(pi => pi.ExchangePoint)
                .HasForeignKey(pi => pi.FatherId);
            modelBuilder.Entity<ExchangePointLine>()
                .HasOne(e => e.Packing)
                .WithMany()
                .HasForeignKey(e => e.PackingId)
                .OnDelete(DeleteBehavior.Restrict);


            // Point Setup
            modelBuilder.Entity<PointSetup>()
                .HasMany(p => p.PointSetupLine)
                .WithOne(pi => pi.PointSetup)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<PointSetup>()
                .HasMany(p => p.PointSetupLine)
                .WithOne(pi => pi.PointSetup)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<PointSetup>()
                .HasMany(p => p.PointSetupCustomer)
                .WithOne(pi => pi.PointSetup)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<PointSetupLine>()
                .HasMany(p => p.Industry)
                .WithOne(gc => gc.Line)
                .HasForeignKey(pi => pi.FatherId);
            modelBuilder.Entity<PointSetupLine>()
                .HasMany(p => p.Brands)
                .WithOne(gc => gc.Line)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<PointSetupLine>()
                .HasMany(p => p.ItemType)
                .WithOne(gc => gc.Line)
                .HasForeignKey(pi => pi.FatherId);
            modelBuilder.Entity<PointSetupLine>()
                .HasMany(p => p.Packings)
                .WithOne(gc => gc.Line)
                .HasForeignKey(pi => pi.FatherId);


            modelBuilder.Entity<Promotion>()
                .HasMany(p => p.PromotionLine)
                .WithOne(pi => pi.Promotion)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<Promotion>()
                .HasMany(p => p.PromotionCustomer)
                .WithOne(pi => pi.Promotion)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<Promotion>()
                .HasMany(p => p.PromotionIndustry)
                .WithOne(pi => pi.Promotion)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<Promotion>()
                .HasMany(p => p.PromotionBrand)
                .WithOne(pi => pi.Promotion)
                .HasForeignKey(pi => pi.FatherId);

            modelBuilder.Entity<Promotion>().HasIndex(g => g.PromotionCode).IsUnique();


            modelBuilder.Entity<PromotionLine>()
                .HasMany(c => c.PromotionLineSub)
                .WithOne(gc => gc.PromotionLine)
                .HasForeignKey(gc => gc.FatherId);

            modelBuilder.Entity<PromotionLineSub>()
                .HasMany(c => c.PromotionItemBuy)
                .WithOne(gc => gc.PromotionLineSub)
                .HasForeignKey(gc => gc.FatherId);

            modelBuilder.Entity<PromotionLineSub>()
                .HasMany(c => c.PromotionUnit)
                .WithOne(gc => gc.PromotionLineSub)
                .HasForeignKey(gc => gc.FatherId);

            modelBuilder.Entity<PromotionLineSub>()
                .HasMany(c => c.PromotionLineSubSub)
                .WithOne(gc => gc.PromotionLineSub)
                .HasForeignKey(gc => gc.FatherId);

            modelBuilder.Entity<PromotionLineSubSub>()
                .HasMany(c => c.PromotionSubItemAdd)
                .WithOne(gc => gc.PromotionLineSubSub)
                .HasForeignKey(gc => gc.FatherId);
            modelBuilder.Entity<PromotionLineSubSub>()
                .HasMany(c => c.PromotionSubUnit)
                .WithOne(gc => gc.PromotionLineSubSub)
                .HasForeignKey(gc => gc.FatherId);


            modelBuilder.Entity<Voucher>()
                .HasMany(p => p.VoucherItem)
                .WithOne(pi => pi.Voucher)
                .HasForeignKey(pi => pi.VoucherId);
            modelBuilder.Entity<Voucher>()
                .HasMany(p => p.VoucherSystem)
                .WithOne(pi => pi.Voucher)
                .HasForeignKey(pi => pi.VoucherId);

            modelBuilder.Entity<Voucher>()
                .HasMany(p => p.VoucherSeller)
                .WithOne(pi => pi.Voucher)
                .HasForeignKey(pi => pi.VoucherId);
            modelBuilder.Entity<Voucher>()
                .HasMany(p => p.VoucherCustomer)
                .WithOne(pi => pi.Voucher)
                .HasForeignKey(pi => pi.VoucherId);

            modelBuilder.Entity<Voucher>()
                .HasMany(p => p.VoucherLine)
                .WithOne(pi => pi.Voucher)
                .HasForeignKey(pi => pi.VoucherId);

            modelBuilder.Entity<VoucherLine>()
                .HasIndex(g => g.VoucherCode)
                .IsUnique();

            modelBuilder.Entity<Coupon>()
                .HasMany(p => p.CouponItem)
                .WithOne(pi => pi.Coupon)
                .HasForeignKey(pi => pi.CouponId);

            modelBuilder.Entity<Coupon>()
                .HasMany(p => p.CouponLine)
                .WithOne(pi => pi.Coupon)
                .HasForeignKey(pi => pi.CouponId);
            modelBuilder.Entity<CouponLine>()
                .HasIndex(g => g.CouponCode)
                .IsUnique();


            //modelBuilder.Entity<Bank>();
            //modelBuilder.Entity<Bank>().HasIndex(u => u.BankCode).IsUnique();
            //modelBuilder.Entity<Bank>().HasIndex(u => u.BankName).IsUnique();
            ////modelBuilder.Entity<Bank>().HasIndex(u => u.GlobalName).IsUnique();
            //var listbank = BankList.GetBanks();
            //modelBuilder.Entity<Bank>().HasData(listbank);

            modelBuilder.Entity<BankCard>()
                .HasIndex(u => new { u.BankId, u.BankCardCode, u.Cardholder })
                .IsUnique();

            modelBuilder.Entity<PaymentMethod>()
                .HasIndex(u => new { u.PaymentMethodCode, u.PaymentMethodName })
                .IsUnique();
            modelBuilder.Entity<PersonInfor>()
                .HasIndex(u => new { u.PersonName, u.Phone, u.PersonType })
                .IsUnique();

            modelBuilder.Entity<Models.Document.ODOC>()
                .HasOne(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .IsRequired(false);

            modelBuilder.Entity<ODOC>()
                .HasMany(p => p.Payments)
                .WithOne()
                .HasForeignKey(p => p.DocId)
                .IsRequired(false);

            modelBuilder.Entity<Payment>()
                .HasMany(p => p.PaymentLine)
                .WithOne(pi => pi.Payment)
                .HasForeignKey(pi => pi.PaymentId);

            modelBuilder.Entity<PersonInfor>()
                .HasIndex(u => new { u.PersonName, u.Phone, u.PersonType })
                .IsUnique();
            //modelBuilder.Entity<TimeZones>();
            //modelBuilder.Entity<TimeZones>().HasIndex(u => u.Name).IsUnique();
            //var listTimeZone = TimeZonesList.GetTimeZones();
            //modelBuilder.Entity<TimeZones>().HasData(listTimeZone);


            modelBuilder.Entity<Branch>()
                .HasMany(p => p.BranchAddress)
                .WithOne(pi => pi.Branch)
                .HasForeignKey(pi => pi.BranchId);

            modelBuilder.Entity<Branch>()
                .HasIndex(u => new { u.BranchName })
                .IsUnique();
            modelBuilder.Entity<Branch>();
            var lisBranch = BranchList.GetBranch();
            modelBuilder.Entity<Branch>().HasData(lisBranch);

            modelBuilder.Entity<Privileges>();
            modelBuilder.Entity<Privileges>()
                .HasIndex(u => new { u.Code })
                .IsUnique();
            var listPrivileges = PrivilegesList.GetPrivileges();
            modelBuilder.Entity<Privileges>().HasData(listPrivileges);

            modelBuilder.Entity<SaleForecastItem>()
                .HasOne(p => p.SaleForecast)
                .WithMany(p => p.SaleForecastItems).HasForeignKey(p => p.SaleForecastId);

            modelBuilder.Entity<SaleForecastItem>()
                .HasOne(p => p.Item)
                .WithMany().HasForeignKey(p => p.ItemId);

            modelBuilder.Entity<SaleForecastItem>()
                .HasMany(p => p.Periods)
                .WithOne().HasForeignKey(p => p.SaleForecastItemId);

            modelBuilder.Entity<SaleForecastItem>()
                .HasOne(p => p.Uom)
                .WithMany().HasForeignKey(p => p.UomId).IsRequired(false);
            modelBuilder.Entity<SaleForecast>().HasOne(p => p.Bp).WithMany().HasForeignKey(p => p.CustomerId);

            modelBuilder.Entity<SaleForecast>().HasOne(p => p.Author).WithMany().HasForeignKey(p => p.UserId);
            modelBuilder.Entity<Industry>()
                .HasMany(x => x.Brands).WithMany(b => b.Industrys)
                .UsingEntity(p => p.ToTable("m2m_BrandIndustry"));
            modelBuilder.Entity<CommittedLineSub>().HasMany<Brand>(p => p.Brand).WithMany(p => p.CommittedLineSub)
                .UsingEntity(p => p.ToTable("m2m_BrandCommittedLineSub"));


            modelBuilder.Entity<Committed>().HasOne(p => p.Creator).WithMany().HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Models.BPGroups.OCRG>().HasMany(o => o.ConditionCusGroups)
                .WithOne().HasForeignKey(p => p.GroupId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            modelBuilder.Entity<Models.BPGroups.ConditionCusGroup>().HasMany(o => o.CondValues)
                .WithOne().HasForeignKey(p => p.CondId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            modelBuilder.Entity<Models.BPGroups.OCRG>().HasMany(p => p.Customers).WithMany(p => p.Groups)
                .UsingEntity(p => p.ToTable("m2m_CustomerGroup"));

            // modelBuilder.Entity<Models.LocationModels.Area>().HasOne(p => p.Region)
            //     .WithMany(p => p.Areas).HasForeignKey(p => p.RegionId);

            modelBuilder.Entity<Models.LocationModels.Region>()
                .HasMany(p => p.Areas)
                .WithMany(p => p.Regions).UsingEntity(p => p.ToTable("m2m_RegionAreas"));


            modelBuilder.Entity<Models.Approval.OWTM>()
                .HasMany(p => p.RUsers)
                .WithMany()
                .UsingEntity(p => p.ToTable("m2m_ApprovalOWTM"));

            modelBuilder.Entity<Models.BusinessPartners.BP>().HasMany(b => b.Classify).WithOne()
                .HasForeignKey(b => b.BpId)
                .IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.BusinessPartners.BpClassify>().HasOne(bp => bp.Region).WithMany()
                .HasForeignKey(p => p.RegionId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.BusinessPartners.BpClassify>().HasOne(bp => bp.Area).WithMany()
                .HasForeignKey(p => p.AreaId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.BusinessPartners.BpClassify>().HasOne(bp => bp.Industry).WithMany()
                .HasForeignKey(p => p.IndustryId);

            modelBuilder.Entity<Models.BusinessPartners.BpClassify>().HasMany(bp => bp.Brands).WithMany()
                .UsingEntity(br => br.ToTable("m2m_BpClassifyBrand"));
            modelBuilder.Entity<Models.BusinessPartners.BpClassify>().HasMany(bp => bp.Sizes).WithMany()
                .UsingEntity(br => br.ToTable("m2m_BpClassifyBpSize"));
            modelBuilder.Entity<Models.BusinessPartners.BpClassify>().HasMany(bp => bp.ItemType).WithMany()
                .UsingEntity(br => br.ToTable("m2m_BpClassifyItemType"));


            modelBuilder.Entity<BP>().HasMany(p => p.CRD6).WithOne().HasForeignKey(p => p.BPId).IsRequired(false);

            modelBuilder.Entity<Models.BusinessPartners.CRD6>().HasOne<AppUser>(p => p.Author).WithMany()
                .HasForeignKey(p => p.AuthorId).IsRequired(false);
            modelBuilder.Entity<Models.Document.DOC14>().HasOne<AppUser>(p => p.Author).WithMany()
                .HasForeignKey(p => p.AuthorId).IsRequired(false);
            modelBuilder.Entity<Models.Document.AttDocument>().HasOne<ODOC>(p => p.Document).WithMany()
                .HasForeignKey(p => p.FatherId).IsRequired(false);
            modelBuilder.Entity<Models.Document.AttDocument>().HasOne<AppUser>(p => p.Author).WithMany()
                .HasForeignKey(p => p.AuthorId).IsRequired(false);

            modelBuilder.Entity<Models.Account.AppUser>().HasOne(p => p.Role).WithMany().HasForeignKey(r => r.RoleId)
                .IsRequired(false);

            modelBuilder.Entity<Models.ItemMasterData.Item>(ent =>
            {
                ent.HasOne(p => p.ProductGroup)
                    .WithMany().HasForeignKey(x => x.ProductGroupCode);
                ent.HasOne(p => p.ProductApplications)
                    .WithMany().HasForeignKey(x => x.ProductApplicationsCode);
                ent.HasOne(p => p.ProductQualityLevel)
                    .WithMany().HasForeignKey(x => x.ProductQualityLevelCode);
            });

            modelBuilder.Entity<CommittedLineSub>(ent =>
            {
                ent.HasMany(p => p.ItemTypes)
                    .WithMany()
                    .UsingEntity(t => t.ToTable("CommittedLineSubLinkItemType"));
            });

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Document)
                .WithMany(o => o.Ratings)
                .HasForeignKey(r => r.DocId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RatingImage>()
                .HasOne(img => img.Rating)
                .WithMany(r => r.Images)
                .HasForeignKey(img => img.RatingId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder
                .Entity<Rating>()
                .Property(r => r.RatingType)
                .HasConversion<string>();
            modelBuilder.Entity<DOC1>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_ItemDetail_OpenQty", "[OpenQty] >= 0"));
            });
        }

        public DbSet<BpClassify> BpClassify { get; set; }

        public static bool IsUniqueAsync<T>(AppDbContext context, Expression<Func<T, bool>> predicate) where T : class
        {
            return context.Set<T>().Any(predicate);
        }
    }
}