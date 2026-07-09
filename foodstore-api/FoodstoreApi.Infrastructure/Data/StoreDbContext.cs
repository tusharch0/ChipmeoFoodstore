using FoodstoreApi.Core.Entities;
using FoodstoreApi.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Infrastructure.Data;

public partial class StoreDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public StoreDbContext() { }

    public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options) { }

    public virtual DbSet<Employee> Employees => Set<Employee>();
    public virtual DbSet<Organization> Organizations => Set<Organization>();
    public virtual DbSet<Branch> Branches => Set<Branch>();
    public virtual DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();
    public virtual DbSet<Customer> Customers => Set<Customer>();
    public virtual DbSet<Category> Categories => Set<Category>();
    public virtual DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public virtual DbSet<Addon> Addons => Set<Addon>();
    public virtual DbSet<MenuItemAddon> MenuItemAddons => Set<MenuItemAddon>();
    public virtual DbSet<Combo> Combos => Set<Combo>();
    public virtual DbSet<ComboItem> ComboItems => Set<ComboItem>();
    public virtual DbSet<Discount> Discounts => Set<Discount>();
    public virtual DbSet<Source> Sources => Set<Source>();
    public virtual DbSet<Order> Orders => Set<Order>();
    public virtual DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public virtual DbSet<OrderItemAddon> OrderItemAddons => Set<OrderItemAddon>();
    public virtual DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public virtual DbSet<Payment> Payments => Set<Payment>();
    public virtual DbSet<PaymentSetting> PaymentSettings => Set<PaymentSetting>();
    public virtual DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public virtual DbSet<Tag> Tags => Set<Tag>();
    public virtual DbSet<BlogPostTag> BlogPostTags => Set<BlogPostTag>();
    public virtual DbSet<Media> Media => Set<Media>();
    public virtual DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    public virtual DbSet<BlogPostCategory> BlogPostCategories => Set<BlogPostCategory>();
    public virtual DbSet<BlogPostRevision> BlogPostRevisions => Set<BlogPostRevision>();
    public virtual DbSet<BlogPostBlock> BlogPostBlocks => Set<BlogPostBlock>();
    public virtual DbSet<BlogSetting> BlogSettings => Set<BlogSetting>();
    public virtual DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public virtual DbSet<EInvoiceProvider> EInvoiceProviders => Set<EInvoiceProvider>();
    public virtual DbSet<EInvoice> EInvoices => Set<EInvoice>();
    public virtual DbSet<EInvoiceSetting> EInvoiceSettings => Set<EInvoiceSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Identity table mapping
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("AspNetUsers");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Banned).HasColumnName("banned");
            entity.ConfigureAudit();
            entity.Property(e => e.EmailConfirmed).HasColumnName("email_verified");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(150).HasColumnName("name");
            entity.Property(e => e.Slug).HasMaxLength(100).HasColumnName("slug");
            entity.Property(e => e.LegalName).HasMaxLength(200).HasColumnName("legal_name");
            entity.Property(e => e.TaxId).HasMaxLength(100).HasColumnName("tax_id");
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).HasColumnName("currency_code").HasDefaultValue("KES");
            entity.Property(e => e.TimeZone).HasMaxLength(100).HasColumnName("time_zone").HasDefaultValue("Africa/Nairobi");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.ConfigureAudit();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("branches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.Name).HasMaxLength(150).HasColumnName("name");
            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity.Property(e => e.Address).HasMaxLength(500).HasColumnName("address");
            entity.Property(e => e.City).HasMaxLength(100).HasColumnName("city");
            entity.Property(e => e.Phone).HasMaxLength(30).HasColumnName("phone");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.ConfigureAudit();
            entity.HasIndex(e => new { e.OrganizationId, e.Code }).IsUnique();
            entity.HasOne(e => e.Organization).WithMany(e => e.Branches).HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrganizationMembership>(entity =>
        {
            entity.ToTable("organization_memberships");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Role).HasMaxLength(50).HasColumnName("role");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.ConfigureAudit();
            entity.HasIndex(e => new { e.OrganizationId, e.UserId }).IsUnique();
            entity.HasOne(e => e.Organization).WithMany(e => e.Memberships).HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany(e => e.OrganizationMemberships).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("AspNetRoles");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.DefaultRoute).HasMaxLength(100).HasColumnName("default_route");
            entity.Property(e => e.IsSystem).HasColumnName("is_system");
            entity.ConfigureAudit();
        });

        // RefreshTokens
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Token).HasMaxLength(500).HasColumnName("token");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.ConfigureAudit();
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Employees
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EmployeeCode).HasMaxLength(50).HasColumnName("employee_code");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.Phone).HasMaxLength(20).HasColumnName("phone");
            entity.Property(e => e.AvatarUrl).HasMaxLength(500).HasColumnName("avatar_url");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
            entity.ConfigureAudit();

            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.EmployeeCode).IsUnique();

            entity.HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Branch)
                .WithMany(e => e.Employees)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Customers
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CustomerCode).HasMaxLength(50).HasColumnName("customer_code");
            entity.Property(e => e.Phone).HasMaxLength(20).HasColumnName("phone");
            entity.Property(e => e.AvatarUrl).HasMaxLength(500).HasColumnName("avatar_url");
            entity.Property(e => e.LoyaltyPoints).HasColumnName("loyalty_points");
            entity.Property(e => e.MembershipLevel).HasMaxLength(50).HasColumnName("membership_level");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.ConfigureAudit();

            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.CustomerCode).IsUnique();

            entity.HasOne(e => e.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Categories
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name").UseCollation("vi_ci_ai");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description").UseCollation("vi_ci_ai");
            entity.Property(e => e.ImageUrl).HasMaxLength(500).HasColumnName("image_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.ConfigureAudit();
        });

        // Menu Items
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.ToTable("menu_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name").UseCollation("vi_ci_ai");
            entity.Property(e => e.Description).HasMaxLength(500).HasColumnName("description").UseCollation("vi_ci_ai");
            entity.Property(e => e.Price).HasColumnType("decimal(10,0)").HasColumnName("price");
            entity.Property(e => e.ImageUrl).HasMaxLength(500).HasColumnName("image_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.ConfigureAudit();

            entity.HasOne(e => e.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Addons
        modelBuilder.Entity<Addon>(entity =>
        {
            entity.ToTable("addons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name").UseCollation("vi_ci_ai");
            entity.Property(e => e.Price).HasColumnType("decimal(10,0)").HasColumnName("price");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.ConfigureAudit();
        });

        // MenuItemAddons
        modelBuilder.Entity<MenuItemAddon>(entity =>
        {
            entity.ToTable("menu_item_addons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MenuItemId).HasColumnName("menu_item_id");
            entity.Property(e => e.AddonId).HasColumnName("addon_id");
            entity.Property(e => e.PriceOverride).HasColumnType("decimal(10,0)").HasColumnName("price_override");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.ConfigureAudit();

            entity.HasIndex(e => new { e.MenuItemId, e.AddonId }).IsUnique();

            entity.HasOne(e => e.MenuItem)
                .WithMany(m => m.MenuItemAddons)
                .HasForeignKey(e => e.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Addon)
                .WithMany(a => a.MenuItemAddons)
                .HasForeignKey(e => e.AddonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Combos
        modelBuilder.Entity<Combo>(entity =>
        {
            entity.ToTable("combos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name").UseCollation("vi_ci_ai");
            entity.Property(e => e.ComboPrice).HasColumnType("decimal(10,0)").HasColumnName("combo_price");
            entity.Property(e => e.Description).HasMaxLength(500).HasColumnName("description").UseCollation("vi_ci_ai");
            entity.Property(e => e.ImageUrl).HasMaxLength(500).HasColumnName("image_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.ConfigureAudit();
        });

        // ComboItems
        modelBuilder.Entity<ComboItem>(entity =>
        {
            entity.ToTable("combo_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComboId).HasColumnName("combo_id");
            entity.Property(e => e.MenuItemId).HasColumnName("menu_item_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.ConfigureAudit();

            entity.HasIndex(e => new { e.ComboId, e.MenuItemId }).IsUnique();

            entity.HasOne(e => e.Combo)
                .WithMany(c => c.ComboItems)
                .HasForeignKey(e => e.ComboId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MenuItem)
                .WithMany(m => m.ComboItems)
                .HasForeignKey(e => e.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Discounts
        modelBuilder.Entity<Discount>(entity =>
        {
            entity.ToTable("discounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name").UseCollation("vi_ci_ai");
            entity.Property(e => e.Type).HasMaxLength(10).HasColumnName("type");
            entity.Property(e => e.Value).HasColumnType("decimal(10,2)").HasColumnName("value");
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(10,0)").HasColumnName("max_discount_amount");
            entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(10,0)").HasColumnName("min_order_amount");
            entity.Property(e => e.UsageLimit).HasColumnName("usage_limit");
            entity.Property(e => e.UsedCount).HasColumnName("used_count");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.ConfigureAudit();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Sources
        modelBuilder.Entity<Source>(entity =>
        {
            entity.ToTable("sources");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("name").UseCollation("vi_ci_ai");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.ConfigureAudit();
            entity.HasIndex(e => e.BranchId);
            entity.HasOne(e => e.Branch).WithMany(e => e.Sources).HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.SetNull);
        });

        // Orders
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderCode).HasMaxLength(20).HasColumnName("order_code");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DiscountId).HasColumnName("discount_id");
            entity.Property(e => e.SubtotalAmount).HasColumnType("decimal(10,0)").HasColumnName("subtotal_amount");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10,0)").HasColumnName("discount_amount");
            entity.Property(e => e.VatAmount).HasColumnType("decimal(10,0)").HasColumnName("vat_amount");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,0)").HasColumnName("total_amount");
            entity.Property(e => e.QrPaymentUrl).HasColumnName("qr_payment_url");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
            entity.Property(e => e.Note).HasMaxLength(500).HasColumnName("note").UseCollation("vi_ci_ai");
            entity.Property(e => e.PrintedAt).HasColumnName("printed_at");
            entity.ConfigureAudit();

            entity.HasIndex(e => e.OrderCode).IsUnique();
            entity.HasIndex(e => e.BranchId);

            entity.HasOne(e => e.Branch)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Source)
                .WithMany(s => s.Orders)
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Employee)
                .WithMany(emp => emp.OrderEmployees)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Discount)
                .WithMany(d => d.Orders)
                .HasForeignKey(e => e.DiscountId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UpdatedByNavigation)
                .WithMany(emp => emp.OrderUpdatedByNavigations)
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Order Items
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.MenuItemId).HasColumnName("menu_item_id");
            entity.Property(e => e.ComboId).HasColumnName("combo_id");
            entity.Property(e => e.MenuItemName).HasMaxLength(255).HasColumnName("menu_item_name").UseCollation("vi_ci_ai");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,0)").HasColumnName("unit_price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,0)").HasColumnName("total_price");
            entity.Property(e => e.Note).HasMaxLength(255).HasColumnName("note").UseCollation("vi_ci_ai");
            entity.ConfigureAudit();

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MenuItem)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(e => e.MenuItemId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Combo)
                .WithMany(c => c.OrderItems)
                .HasForeignKey(e => e.ComboId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Order Item Addons
        modelBuilder.Entity<OrderItemAddon>(entity =>
        {
            entity.ToTable("order_item_addons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.AddonId).HasColumnName("addon_id");
            entity.Property(e => e.AddonName).HasMaxLength(100).HasColumnName("addon_name").UseCollation("vi_ci_ai");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,0)").HasColumnName("unit_price");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,0)").HasColumnName("total_price");
            entity.ConfigureAudit();

            entity.HasOne(e => e.OrderItem)
                .WithMany(oi => oi.OrderItemAddons)
                .HasForeignKey(e => e.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Addon)
                .WithMany(a => a.OrderItemAddons)
                .HasForeignKey(e => e.AddonId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Order Status History
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("order_status_history");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.FromStatus).HasMaxLength(20).HasColumnName("from_status");
            entity.Property(e => e.ToStatus).HasMaxLength(20).HasColumnName("to_status");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            entity.Property(e => e.Note).HasMaxLength(500).HasColumnName("note").UseCollation("vi_ci_ai");

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderStatusHistories)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ChangedByNavigation)
                .WithMany(emp => emp.OrderStatusHistories)
                .HasForeignKey(e => e.ChangedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Payments
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Amount).HasColumnType("decimal(10,0)").HasColumnName("amount");
            entity.Property(e => e.Method).HasMaxLength(20).HasColumnName("method");
            entity.Property(e => e.ReferenceCode).HasMaxLength(100).HasColumnName("reference_code");
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.ConfigureAudit();

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Payment Settings
        modelBuilder.Entity<PaymentSetting>(entity =>
        {
            entity.ToTable("payment_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BankId).HasMaxLength(50).HasColumnName("bank_id");
            entity.Property(e => e.BankAccount).HasMaxLength(50).HasColumnName("bank_account");
            entity.Property(e => e.BankName).HasMaxLength(100).HasColumnName("bank_name").UseCollation("vi_ci_ai");
            entity.Property(e => e.BankAccountName).HasMaxLength(200).HasColumnName("bank_account_name");
            entity.Property(e => e.Template).HasMaxLength(20).HasColumnName("template").HasDefaultValue("compact2");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.ConfigureAudit();
        });

        // Blog Posts
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.ToTable("blog_posts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasMaxLength(255).HasColumnName("title").UseCollation("vi_ci_ai");
            entity.Property(e => e.Slug).HasMaxLength(255).HasColumnName("slug");
            entity.Property(e => e.Excerpt).HasMaxLength(500).HasColumnName("excerpt").UseCollation("vi_ci_ai");
            entity.Property(e => e.Content).HasColumnName("content").UseCollation("vi_ci_ai");
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500).HasColumnName("thumbnail_url");
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status").HasDefaultValue("draft");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.MetaTitle).HasMaxLength(100).HasColumnName("meta_title");
            entity.Property(e => e.MetaDescription).HasMaxLength(200).HasColumnName("meta_description");
            entity.Property(e => e.FocusKeyword).HasMaxLength(100).HasColumnName("focus_keyword");
            entity.Property(e => e.Keywords).HasMaxLength(500).HasColumnName("keywords");
            entity.Property(e => e.CanonicalUrl).HasMaxLength(500).HasColumnName("canonical_url");
            entity.Property(e => e.OgImageUrl).HasMaxLength(500).HasColumnName("og_image_url");
            entity.Property(e => e.ReadingTime).HasColumnName("reading_time").HasDefaultValue(0);
            entity.Property(e => e.WordCount).HasColumnName("word_count").HasDefaultValue(0);
            entity.Property(e => e.SeoScore).HasColumnName("seo_score").HasDefaultValue(0);
            entity.Property(e => e.ScheduledAt).HasColumnName("scheduled_at");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured").HasDefaultValue(false);
            entity.Property(e => e.Template).HasMaxLength(100).HasColumnName("template").HasDefaultValue("default");
            entity.Property(e => e.ViewCount).HasColumnName("view_count").HasDefaultValue(0);
            entity.Property(e => e.AllowComments).HasColumnName("allow_comments").HasDefaultValue(true);
            entity.ConfigureAudit();

            entity.HasOne(e => e.Author)
                .WithMany(emp => emp.BlogPosts)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ReviewedByNavigation)
                .WithMany()
                .HasForeignKey(e => e.ReviewedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Tags
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name").UseCollation("vi_ci_ai");
            entity.Property(e => e.Slug).HasMaxLength(100).HasColumnName("slug");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description").UseCollation("vi_ci_ai");
            entity.Property(e => e.Color).HasMaxLength(7).HasColumnName("color").HasDefaultValue("#f59e0b");
            entity.ConfigureAudit();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // BlogPostTags
        modelBuilder.Entity<BlogPostTag>(entity =>
        {
            entity.ToTable("blog_post_tags");
            entity.HasKey(e => new { e.PostId, e.TagId });
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.BlogPostTags)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.BlogPostTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.ConfigureAudit();
        });

        // Media
        modelBuilder.Entity<Media>(entity =>
        {
            entity.ToTable("media");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FileName).HasMaxLength(255).HasColumnName("file_name");
            entity.Property(e => e.Folder).HasMaxLength(100).HasDefaultValue("misc").HasColumnName("folder");
            entity.Property(e => e.FileUrl).HasMaxLength(500).HasColumnName("file_url");
            entity.Property(e => e.FileType).HasMaxLength(50).HasColumnName("file_type");
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.AltText).HasMaxLength(255).HasColumnName("alt_text");
            entity.Property(e => e.UploadedByEmployee).HasColumnName("uploaded_by_employee");
            entity.Property(e => e.UploadedByCustomer).HasColumnName("uploaded_by_customer");
            entity.Property(e => e.EntityType).HasMaxLength(50).HasColumnName("entity_type");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.ConfigureAudit();

            entity.HasOne(e => e.UploadedByEmployeeNavigation)
                .WithMany()
                .HasForeignKey(e => e.UploadedByEmployee)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UploadedByCustomerNavigation)
                .WithMany()
                .HasForeignKey(e => e.UploadedByCustomer)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Blog Categories
        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.ToTable("blog_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name").UseCollation("vi_ci_ai");
            entity.Property(e => e.Slug).HasMaxLength(100).HasColumnName("slug");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description").UseCollation("vi_ci_ai");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.ConfigureAudit();
            entity.HasIndex(e => e.Slug).IsUnique();

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Blog Post Categories
        modelBuilder.Entity<BlogPostCategory>(entity =>
        {
            entity.ToTable("blog_post_categories");
            entity.HasKey(e => new { e.PostId, e.CategoryId });
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.BlogPostCategories)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.BlogPostCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Blog Post Revisions
        modelBuilder.Entity<BlogPostRevision>(entity =>
        {
            entity.ToTable("blog_post_revisions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.Title).HasMaxLength(255).HasColumnName("title");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Excerpt).HasMaxLength(500).HasColumnName("excerpt");
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500).HasColumnName("thumbnail_url");
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
            entity.Property(e => e.MetaTitle).HasMaxLength(100).HasColumnName("meta_title");
            entity.Property(e => e.MetaDescription).HasMaxLength(200).HasColumnName("meta_description");
            entity.Property(e => e.FocusKeyword).HasMaxLength(100).HasColumnName("focus_keyword");
            entity.Property(e => e.Keywords).HasMaxLength(500).HasColumnName("keywords");
            entity.Property(e => e.CanonicalUrl).HasMaxLength(500).HasColumnName("canonical_url");
            entity.Property(e => e.OgImageUrl).HasMaxLength(500).HasColumnName("og_image_url");
            entity.Property(e => e.WordCount).HasColumnName("word_count");
            entity.Property(e => e.ReadingTime).HasColumnName("reading_time");
            entity.Property(e => e.BlocksJson).HasColumnName("blocks_json");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.BlogPostRevisions)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PostId, e.CreatedAt });
        });

        // Blog Post Blocks
        modelBuilder.Entity<BlogPostBlock>(entity =>
        {
            entity.ToTable("blog_post_blocks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.BlockType).HasMaxLength(50).HasColumnName("block_type");
            entity.Property(e => e.BlockData).HasColumnName("block_data");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.BlogPostBlocks)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Blog Settings
        modelBuilder.Entity<BlogSetting>(entity =>
        {
            entity.ToTable("blog_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Key).HasMaxLength(100).HasColumnName("key");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => e.Key).IsUnique();
        });

        // E-Invoice Providers
        modelBuilder.Entity<EInvoiceProvider>(entity =>
        {
            entity.ToTable("e_invoice_providers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name").UseCollation("vi_ci_ai");
            entity.Property(e => e.ProviderType).HasMaxLength(50).HasColumnName("provider_type");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ConfigJson).HasColumnName("config_json");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description").UseCollation("vi_ci_ai");
            entity.ConfigureAudit();
        });

        // E-Invoices
        modelBuilder.Entity<EInvoice>(entity =>
        {
            entity.ToTable("e_invoices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).HasColumnName("invoice_number");
            entity.Property(e => e.TemplateCode).HasMaxLength(20).HasColumnName("template_code");
            entity.Property(e => e.SerialNumber).HasMaxLength(20).HasColumnName("serial_number");
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status").HasDefaultValue("draft");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,0)").HasColumnName("total_amount");
            entity.Property(e => e.VatAmount).HasColumnType("decimal(10,0)").HasColumnName("vat_amount");
            entity.Property(e => e.BuyerName).HasMaxLength(255).HasColumnName("buyer_name").UseCollation("vi_ci_ai");
            entity.Property(e => e.BuyerTaxCode).HasMaxLength(20).HasColumnName("buyer_tax_code");
            entity.Property(e => e.BuyerAddress).HasMaxLength(500).HasColumnName("buyer_address").UseCollation("vi_ci_ai");
            entity.Property(e => e.PdfUrl).HasMaxLength(500).HasColumnName("pdf_url");
            entity.Property(e => e.XmlUrl).HasMaxLength(500).HasColumnName("xml_url");
            entity.Property(e => e.ProviderResponse).HasColumnName("provider_response");
            entity.Property(e => e.ErrorMessage).HasMaxLength(500).HasColumnName("error_message");
            entity.Property(e => e.IssuedAt).HasColumnName("issued_at");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CancelReason).HasMaxLength(255).HasColumnName("cancel_reason");
            entity.ConfigureAudit();

            entity.HasIndex(e => e.OrderId).IsUnique();
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();

            entity.HasOne(e => e.Order)
                .WithOne()
                .HasForeignKey<EInvoice>(e => e.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // E-Invoice Settings
        modelBuilder.Entity<EInvoiceSetting>(entity =>
        {
            entity.ToTable("e_invoice_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DefaultProviderId).HasColumnName("default_provider_id");
            entity.Property(e => e.AutoIssue).HasColumnName("auto_issue");
            entity.Property(e => e.DefaultTemplateCode).HasMaxLength(20).HasColumnName("default_template_code");
            entity.Property(e => e.DefaultSerialNumber).HasMaxLength(20).HasColumnName("default_serial_number");
            entity.Property(e => e.DigitalSignatureConfig).HasColumnName("digital_signature_config");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.DefaultProvider)
                .WithMany()
                .HasForeignKey(e => e.DefaultProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
