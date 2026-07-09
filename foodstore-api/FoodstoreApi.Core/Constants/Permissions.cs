namespace FoodstoreApi.Core.Constants;

public static class Permissions
{
    public static readonly List<PermissionInfo> All = new()
    {
        // Organization and branch management
        new("organization.view", "View organizations", "Can view restaurant organizations and branches", "organization"),
        new("organization.manage", "Manage organizations", "Can create and manage restaurant organizations and branches", "organization"),

        // Category
        new("category.view",   "Xem danh mục",   "Có thể xem danh sách danh mục",  "category"),
        new("category.create", "Tạo danh mục",   "Có thể tạo danh mục mới",        "category"),
        new("category.update", "Sửa danh mục",   "Có thể sửa danh mục",            "category"),
        new("category.delete", "Xóa danh mục",   "Có thể xóa danh mục",            "category"),

        // Menu
        new("menu.view",   "Xem món ăn",   "Có thể xem danh sách món",  "menu"),
        new("menu.create", "Tạo món ăn",   "Có thể tạo món mới",        "menu"),
        new("menu.update", "Sửa món ăn",   "Có thể sửa món",            "menu"),
        new("menu.delete", "Xóa món ăn",   "Có thể xóa món",            "menu"),

        // Order
        new("order.view",   "Xem đơn hàng",   "Có thể xem đơn",                  "order"),
        new("order.create", "Tạo đơn hàng",   "Có thể tạo đơn",                   "order"),
        new("order.update", "Cập nhật đơn",   "Có thể cập nhật trạng thái đơn",  "order"),
        new("order.delete", "Hủy đơn hàng",   "Có thể hủy đơn",                  "order"),

        // Employee
        new("employee.view",   "Xem nhân viên",      "Có thể xem danh sách nhân viên",  "employee"),
        new("employee.create", "Tạo nhân viên",      "Có thể tạo nhân viên mới",       "employee"),
        new("employee.update", "Sửa nhân viên",      "Có thể sửa thông tin nhân viên", "employee"),
        new("employee.delete", "Xóa nhân viên",      "Có thể xóa nhân viên",           "employee"),

        // Role
        new("role.view",   "Xem vai trò",   "Có thể xem vai trò",      "role"),
        new("role.create", "Tạo vai trò",   "Có thể tạo vai trò mới",  "role"),
        new("role.update", "Sửa vai trò",   "Có thể sửa vai trò",      "role"),
        new("role.delete", "Xóa vai trò",   "Có thể xóa vai trò",      "role"),

        // Discount
        new("discount.view",   "Xem giảm giá",   "Có thể xem mã giảm giá",  "discount"),
        new("discount.create", "Tạo giảm giá",   "Có thể tạo mã giảm giá",  "discount"),
        new("discount.update", "Sửa giảm giá",   "Có thể sửa mã giảm giá",  "discount"),
        new("discount.delete", "Xóa giảm giá",   "Có thể xóa mã giảm giá",  "discount"),

        // Source
        new("source.view",   "Xem nguồn đơn",   "Có thể xem danh sách nguồn đơn",  "source"),
        new("source.create", "Tạo nguồn đơn",   "Có thể tạo nguồn đơn mới",        "source"),
        new("source.update", "Sửa nguồn đơn",   "Có thể sửa thông tin nguồn đơn",  "source"),
        new("source.delete", "Xóa nguồn đơn",   "Có thể xóa nguồn đơn",            "source"),

        // Addon
        new("addon.view",   "Xem topping",   "Có thể xem topping",  "addon"),
        new("addon.create", "Tạo topping",   "Có thể tạo topping",  "addon"),
        new("addon.update", "Sửa topping",   "Có thể sửa topping",  "addon"),
        new("addon.delete", "Xóa topping",   "Có thể xóa topping",  "addon"),

        // Combo
        new("combo.view",   "Xem combo",   "Có thể xem combo",  "combo"),
        new("combo.create", "Tạo combo",   "Có thể tạo combo",  "combo"),
        new("combo.update", "Sửa combo",   "Có thể sửa combo",  "combo"),
        new("combo.delete", "Xóa combo",   "Có thể xóa combo",  "combo"),

        // Dashboard
        new("dashboard.view", "Xem dashboard", "Có thể xem trang tổng quan", "dashboard"),

        // Analytics
        new("analytics.view",   "Xem phân tích", "Có thể xem báo cáo và dự báo", "analytics"),
        new("analytics.export", "Xuất báo cáo",  "Có thể xuất file báo cáo",    "analytics"),

        // Payment
        new("payment.view",   "Xem cài đặt thanh toán",   "Có thể xem thông tin tài khoản ngân hàng", "payment"),
        new("payment.update", "Cập nhật thanh toán",      "Có thể thêm/sửa tài khoản ngân hàng",     "payment"),
        new("payment.delete", "Xóa cài đặt thanh toán",   "Có thể xóa tài khoản ngân hàng",          "payment"),

        // System
        new("system.view",   "Xem cài đặt hệ thống",       "Có thể xem cấu hình hệ thống",            "system"),
        new("system.update", "Cập nhật cài đặt hệ thống",  "Có thể thay đổi cấu hình hệ thống",       "system"),

        // Blog
        new("blog.view",   "Xem bài viết",   "Có thể xem danh sách bài viết",  "blog"),
        new("blog.create", "Tạo bài viết",   "Có thể tạo bài viết mới",        "blog"),
        new("blog.update", "Sửa bài viết",   "Có thể sửa bài viết",            "blog"),
        new("blog.delete", "Xóa bài viết",   "Có thể xóa bài viết",            "blog"),

        // Customer
        new("customer.view",   "Xem khách hàng",      "Có thể xem danh sách khách hàng",  "customer"),
        new("customer.create", "Tạo khách hàng",      "Có thể tạo khách hàng",            "customer"),
        new("customer.update", "Sửa khách hàng",      "Có thể sửa thông tin khách hàng",  "customer"),
        new("customer.delete", "Xóa khách hàng",      "Có thể xóa khách hàng",            "customer"),

        // Media
        new("media.upload", "Tải ảnh lên",  "Có thể tải ảnh lên hệ thống",   "media"),
        new("media.delete", "Xóa ảnh",      "Có thể xóa ảnh khỏi hệ thống",  "media"),

        // E-Invoice
        new("einvoice.view",         "Xem hóa đơn điện tử",    "Có thể xem danh sách hóa đơn điện tử",  "einvoice"),
        new("einvoice.providers",    "Quản lý nhà cung cấp",   "Có thể xem/quản lý nhà cung cấp HĐĐT", "einvoice"),
        new("einvoice.issue",        "Phát hành hóa đơn",      "Có thể phát hành hóa đơn điện tử",     "einvoice"),
        new("einvoice.cancel",       "Hủy hóa đơn",            "Có thể hủy hóa đơn điện tử",           "einvoice"),
        new("einvoice.settings",     "Cài đặt HĐĐT",           "Có thể xem/sửa cài đặt HĐĐT",          "einvoice"),
    };

    public static List<string> AllCodes => All.Select(p => p.Code).ToList();
}

public record PermissionInfo(string Code, string Name, string Description, string Module);
