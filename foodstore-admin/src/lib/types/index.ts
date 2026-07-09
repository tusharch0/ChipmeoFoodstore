export interface Category {
  id: string;
  name: string;
  description?: string;
  imageUrl?: string | null;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CategoryCreateDto {
  name: string;
  description?: string;
  imageUrl?: string | null;
  isActive: boolean;
}

export interface CategoryUpdateDto {
  name: string;
  description?: string;
  imageUrl?: string | null;
  isActive: boolean;
}

export interface MenuItem {
  id: string;
  name: string;
  description?: string;
  price: number;
  categoryId: string;
  categoryName?: string;
  imageUrl?: string | null;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
  addons?: Addon[];
}

export interface MenuItemCreateDto {
  name: string;
  description?: string;
  price: number;
  categoryId: string;
  imageUrl?: string | null;
  isActive: boolean;
  addonIds?: string[];
}

export interface MenuItemUpdateDto {
  name: string;
  description?: string;
  price: number;
  categoryId: string;
  imageUrl?: string | null;
  isActive: boolean;
  addonIds?: string[];
}

export interface Addon {
  id: string;
  name: string;
  price: number;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface AddonCreateDto {
  name: string;
  price: number;
  isActive: boolean;
}

export interface AddonUpdateDto {
  name: string;
  price: number;
  isActive: boolean;
}

export interface Combo {
  id: string;
  name: string;
  description?: string;
  imageUrl?: string | null;
  comboPrice: number;
  items: ComboItem[];
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface ComboItem {
  menuItemId: string;
  quantity: number;
}

export interface ComboCreateDto {
  name: string;
  description?: string;
  imageUrl?: string | null;
  comboPrice: number;
  items: ComboItem[];
  isActive: boolean;
}

export interface ComboUpdateDto {
  name: string;
  description?: string;
  imageUrl?: string | null;
  comboPrice: number;
  items: ComboItem[];
  isActive: boolean;
}

export interface Discount {
  id: string;
  code: string;
  name: string;
  description?: string;
  type: "percent" | "amount";
  value: number;
  maxDiscountAmount?: number;
  minOrderAmount?: number;
  usageLimit?: number;
  usedCount?: number;
  startDate?: string;
  endDate?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface DiscountCreateDto {
  code: string;
  name: string;
  description?: string;
  type: "percent" | "amount";
  value: number;
  maxDiscountAmount?: number | null;
  minOrderAmount?: number | null;
  usageLimit?: number | null;
  startDate?: string | null;
  endDate?: string | null;
  isActive: boolean;
}

export interface DiscountUpdateDto {
  code: string;
  name: string;
  description?: string;
  type: "percent" | "amount";
  value: number;
  maxDiscountAmount?: number | null;
  minOrderAmount?: number | null;
  usageLimit?: number | null;
  startDate?: string | null;
  endDate?: string | null;
  isActive: boolean;
}

export interface Order {
  id: string;
  orderCode: string;
  sourceId?: string;
  sourceName?: string;
  customerId?: string;
  items: OrderItem[];
  subtotalAmount: number;
  discountId?: string;
  discountAmount: number;
  discountCode?: string;
  vatAmount?: number;
  totalAmount: number;
  status: OrderStatus;
  note?: string;
  customerName?: string;
  customerPhone?: string;
  history?: OrderStatusHistory[];
  qrPaymentUrl?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface OrderItem {
  id?: string;
  menuItemId?: string;
  menuItemName?: string;
  comboId?: string;
  comboName?: string;
  quantity: number;
  unitPrice: number;
  totalPrice?: number;
  addons?: OrderAddon[];
  note?: string;
}

export interface OrderAddon {
  addonId: string;
  addonName?: string;
  quantity: number;
  unitPrice: number;
}

export type OrderStatus =
  | "pending"
  | "confirmed"
  | "preparing"
  | "ready"
  | "served"
  | "paid"
  | "cancelled";

export interface OrderCreateDto {
  sourceId?: string;
  customerName?: string;
  customerPhone?: string;
  items: OrderItemCreateDto[];
  discountId?: string;
  discountCode?: string;
  note?: string;
}

export interface OrderItemCreateDto {
  menuItemId?: string;
  comboId?: string;
  quantity: number;
  addons?: { addonId: string; quantity: number }[];
  note?: string;
}

export interface OrderUpdateStatusDto {
  status: string;
  paymentMethod?: string;
  paymentAmount?: number;
}

export interface OrderStatusHistory {
  id: string;
  orderId: string;
  fromStatus: string;
  toStatus: string;
  note?: string;
  changedBy: string;
  changedAt: string;
  changedByName?: string;
}

export interface PagedOrdersResponse {
  items: Order[];
  totalCount: number;
  totalPages: number;
}

export interface Employee {
  id: string;
  fullName: string;
  username: string;
  email?: string;
  phone?: string;
  avatarUrl?: string | null;
  roleId: string;
  roleName?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface EmployeeCreateDto {
  fullName: string;
  username?: string;
  password: string;
  email?: string;
  phone?: string;
  avatarUrl?: string | null;
  roleId: string;
  isActive: boolean;
}

export interface EmployeeUpdateDto {
  fullName: string;
  username?: string;
  email?: string;
  phone?: string;
  avatarUrl?: string | null;
  roleId: string;
  isActive: boolean;
}

export interface Role {
  id: string;
  name: string;
  description?: string;
  defaultRoute?: string;
  isActive: boolean;
  permissionCodes?: string[];
  createdAt?: string;
  updatedAt?: string;
}

export interface RoleCreateDto {
  name: string;
  description?: string;
  defaultRoute?: string;
  isActive: boolean;
}

export interface RoleUpdateDto {
  name: string;
  description?: string;
  defaultRoute?: string;
  isActive: boolean;
}

export interface Permission {
  code: string;
  name: string;
  description?: string;
  module: string;
}

export interface Source {
  id: string;
  name: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface SourceCreateDto {
  name: string;
  isActive: boolean;
}

export interface SourceUpdateDto {
  name: string;
  isActive: boolean;
}

export interface Customer {
  id: string;
  userId: string;
  customerCode: string;
  name: string;
  username: string;
  email: string;
  phone?: string;
  avatarUrl?: string;
  loyaltyPoints: number;
  membershipLevel?: string;
  birthday?: string;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface CreateCustomerDto {
  name: string;
  username?: string;
  email?: string;
  password?: string;
  phone?: string;
  birthday?: string;
  isActive?: boolean;
}

export interface UpdateCustomerAdminDto {
  name?: string;
  phone?: string;
  avatarUrl?: string;
  birthday?: string;
  isActive?: boolean;
  points?: number;
}

export interface AddPointsDto {
  points: number;
  reason: string;
}

export interface CustomerOrderHistory {
  id: string;
  orderCode?: string;
  createdAt: string;
  totalAmount?: number;
  status?: string;
}

export interface CustomerBirthday {
  id: string;
  customerCode: string;
  name: string;
  phone?: string;
  birthday: string;
  membershipLevel?: string;
}

export interface UpcomingBirthdays {
  thisWeek: CustomerBirthday[];
  thisMonth: CustomerBirthday[];
  totalThisWeek: number;
  totalThisMonth: number;
}

export interface PaymentSetting {
  id: string;
  bankId: string;
  bankAccount: string;
  bankName: string;
  bankAccountName?: string;
  template: string;
  isDefault: boolean;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface PaymentSettingCreateDto {
  bankId: string;
  bankAccount: string;
  bankName: string;
  bankAccountName?: string;
  template: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface PaymentSettingUpdateDto {
  bankId: string;
  bankAccount: string;
  bankName: string;
  bankAccountName?: string;
  template: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface DashboardStats {
  today: { revenue: number; orders: number };
  month: { revenue: number; orders: number };
  total: { revenue: number; orders: number };
  popularItems: PopularItem[];
  popularCombos: PopularCombo[];
  last7Days: DailyStats[];
  last6Months: MonthlyStats[];
  averageOrderValue: number;
  peakHour: string;
  paymentMethodBreakdown: Record<string, number>;
  totalCustomers: number;
  serviceTypeStats: PopularItem[];
}

export interface PopularItem {
  name: string;
  quantity: number;
  revenue: number;
}

export interface PopularCombo {
  name: string;
  quantity: number;
  revenue: number;
}

export interface DailyStats {
  date: string;
  revenue: number;
  orders: number;
}

export interface MonthlyStats {
  month: string;
  revenue: number;
  orders: number;
}

export interface Analytics {
  totalRevenue: number;
  totalVat: number;
  totalOrders: number;
  revenueChart: ChartData[];
  ordersChart: ChartData[];
  topItems: TopItem[];
  popularCombos: PopularCombo[];
}

export interface ChartData {
  label: string;
  value: number;
}

export interface TopItem {
  name: string;
  sold: number;
}

// ============== CMS Types ==============
export interface BlogPost {
  id: string;
  title: string;
  slug: string;
  excerpt?: string;
  content?: string;
  thumbnailUrl?: string;
  status?: string;
  authorId?: string;
  authorName?: string;
  publishedAt?: string;
  metaTitle?: string;
  metaDescription?: string;
  focusKeyword?: string;
  keywords?: string;
  canonicalUrl?: string;
  ogImageUrl?: string;
  readingTime?: number;
  wordCount?: number;
  seoScore?: number;
  scheduledAt?: string;
  reviewedBy?: string;
  reviewedByName?: string;
  reviewedAt?: string;
  isFeatured: boolean;
  template?: string;
  viewCount: number;
  allowComments: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
  tags?: TagDto[];
  categories?: BlogCategoryDto[];
  blocks?: BlogPostBlockDto[];
}

export interface TagDto {
  id: string;
  name: string;
  slug: string;
  description?: string;
  color: string;
  postCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTagDto {
  name: string;
  description?: string;
  color?: string;
}

export interface UpdateTagDto {
  name?: string;
  description?: string;
  color?: string;
}

export interface BlogCategoryDto {
  id: string;
  name: string;
  slug: string;
  description?: string;
  parentId?: string;
  parentName?: string;
  sortOrder: number;
  postCount: number;
  createdAt: string;
}

export interface CreateBlogCategoryDto {
  name: string;
  description?: string;
  parentId?: string;
  sortOrder?: number;
}

export interface UpdateBlogCategoryDto {
  name?: string;
  description?: string;
  parentId?: string;
  sortOrder?: number;
}

export interface CreateBlogPostDto {
  title: string;
  excerpt?: string;
  content?: string;
  thumbnailUrl?: string;
  status?: string;
  metaTitle?: string;
  metaDescription?: string;
  focusKeyword?: string;
  keywords?: string;
  canonicalUrl?: string;
  ogImageUrl?: string;
  scheduledAt?: string;
  isFeatured?: boolean;
  template?: string;
  allowComments?: boolean;
  tagIds?: string[];
  categoryIds?: string[];
  blocks?: CreateBlogBlockDto[];
}

export interface UpdateBlogPostDto {
  title?: string;
  excerpt?: string;
  content?: string;
  thumbnailUrl?: string;
  status?: string;
  metaTitle?: string;
  metaDescription?: string;
  focusKeyword?: string;
  keywords?: string;
  canonicalUrl?: string;
  ogImageUrl?: string;
  scheduledAt?: string;
  isFeatured?: boolean;
  template?: string;
  allowComments?: boolean;
  tagIds?: string[];
  categoryIds?: string[];
  blocks?: CreateBlogBlockDto[];
}

export interface BlogPostBlockDto {
  id: string;
  postId: string;
  blockType: string;
  blockData: string;
  sortOrder: number;
}

export interface CreateBlogBlockDto {
  blockType: string;
  blockData: string;
  sortOrder?: number;
}

export interface BlogPostRevisionDto {
  id: string;
  postId: string;
  title: string;
  content?: string;
  excerpt?: string;
  thumbnailUrl?: string;
  status?: string;
  blocksJson?: string;
  createdBy?: string;
  createdByName?: string;
  createdAt: string;
}

export interface BlogSettingDto {
  id: string;
  key: string;
  value: string;
  description?: string;
  updatedAt: string;
}

export interface UpdateBlogSettingDto {
  value: string;
  description?: string;
}

export interface CmsDashboardStats {
  totalPosts: number;
  publishedPosts: number;
  draftPosts: number;
  scheduledPosts: number;
  totalCategories: number;
  totalTags: number;
  totalViews: number;
  recentPostsCount: number;
  featuredPostsCount: number;
}

export interface Forecast {
  forecasts: ForecastData[];
}

export interface ForecastData {
  date: string;
  revenue: number;
}

export interface Recommendation {
  item1Name: string;
  item2Name: string;
  frequency: number;
  totalOriginalPrice: number;
  suggestedPrice: number;
  reason: string;
}

// ============== E-Invoice Types ==============
export interface EInvoiceProvider {
  id: string;
  name: string;
  providerType: string;
  isActive: boolean;
  config: Record<string, unknown>;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEInvoiceProviderDto {
  name: string;
  providerType: string;
  isActive: boolean;
  config: Record<string, unknown>;
  description?: string;
}

export interface UpdateEInvoiceProviderDto {
  name: string;
  providerType: string;
  isActive: boolean;
  config: Record<string, unknown>;
  description?: string;
}

export interface EInvoice {
  id: string;
  orderId: string;
  orderCode?: string;
  providerId?: string;
  providerName?: string;
  invoiceNumber?: string;
  templateCode?: string;
  serialNumber?: string;
  status: string;
  totalAmount: number;
  vatAmount: number;
  buyerName?: string;
  buyerTaxCode?: string;
  buyerAddress?: string;
  pdfUrl?: string;
  xmlUrl?: string;
  errorMessage?: string;
  issuedAt?: string;
  cancelledAt?: string;
  cancelReason?: string;
  createdAt: string;
}

export interface IssueInvoiceDto {
  providerId?: string;
  templateCode?: string;
  serialNumber?: string;
  buyerName?: string;
  buyerTaxCode?: string;
  buyerAddress?: string;
}

export interface CancelInvoiceDto {
  reason: string;
}

export interface EInvoiceSetting {
  id: string;
  defaultProviderId?: string;
  defaultProviderName?: string;
  autoIssue: boolean;
  defaultTemplateCode?: string;
  defaultSerialNumber?: string;
  digitalSignatureConfig?: Record<string, unknown>;
  updatedAt: string;
}

export interface UpdateEInvoiceSettingDto {
  defaultProviderId?: string;
  autoIssue: boolean;
  defaultTemplateCode?: string;
  defaultSerialNumber?: string;
  digitalSignatureConfig?: Record<string, unknown>;
}

export interface EInvoiceDashboard {
  totalInvoices: number;
  draftInvoices: number;
  issuedInvoices: number;
  failedInvoices: number;
  cancelledInvoices: number;
  activeProviders: number;
}

export interface PaymentIntent {
  id: string; orderId: string; orderCode?: string; amount: number; currency: string; provider: string;
  providerReference?: string; checkoutId?: string; status: string; customerPhone?: string;
  failureReason?: string; expiresAt?: string; createdAt: string; updatedAt: string;
}

export interface PaymentEvent {
  id: string; provider: string; providerEventId: string; eventType: string; signatureValid: boolean;
  processingOutcome?: string; attempts: number; receivedAt: string; processedAt?: string;
}

export interface PaymentIntentDetail extends PaymentIntent { events: PaymentEvent[] }
