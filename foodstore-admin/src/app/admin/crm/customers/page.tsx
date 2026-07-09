"use client"

import * as React from "react"
import { Edit, Plus, QrCode, Trash2, Coins, History } from "lucide-react"
import { QRCodeSVG } from "qrcode.react"
import { toast } from "sonner"
import type { ColumnDef } from "@tanstack/react-table"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Sheet, SheetContent, SheetHeader, SheetTitle } from "@/components/ui/sheet"
import { DataTable } from "@/components/data-table"
import { CrudSheet } from "@/components/crud-sheet"
import { DeleteConfirmDialog } from "@/components/confirm-dialog"
import { StatusBadge } from "@/components/status-badge"
import { customerService } from "@/lib/services/customer-service"
import { formatDateTime, formatCurrency, formatDate } from "@/lib/utils"
import type { Customer, CreateCustomerDto, UpdateCustomerAdminDto, AddPointsDto, CustomerOrderHistory } from "@/lib/types"

export default function CustomersPage() {
  const [data, setData] = React.useState<Customer[]>([])
  const [loading, setLoading] = React.useState(true)
  const [sheetOpen, setSheetOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<Customer | null>(null)
  const [submitting, setSubmitting] = React.useState(false)
  const [deleteOpen, setDeleteOpen] = React.useState(false)
  const [deleting, setDeleting] = React.useState(false)
  const [deleteTarget, setDeleteTarget] = React.useState<Customer | null>(null)
  const [qrTarget, setQrTarget] = React.useState<Customer | null>(null)
  const [pointsOpen, setPointsOpen] = React.useState(false)
  const [pointsTarget, setPointsTarget] = React.useState<Customer | null>(null)
  const [pointsAmount, setPointsAmount] = React.useState(0)
  const [pointsReason, setPointsReason] = React.useState("")
  const [pointsSubmitting, setPointsSubmitting] = React.useState(false)
  const [orderHistoryOpen, setOrderHistoryOpen] = React.useState(false)
  const [orderHistoryTarget, setOrderHistoryTarget] = React.useState<Customer | null>(null)
  const [orderHistoryData, setOrderHistoryData] = React.useState<CustomerOrderHistory[]>([])
  const [orderHistoryLoading, setOrderHistoryLoading] = React.useState(false)

  const [formName, setFormName] = React.useState("")
  const [formPhone, setFormPhone] = React.useState("")
  const [formEmail, setFormEmail] = React.useState("")
  const [formUsername, setFormUsername] = React.useState("")
  const [formPassword, setFormPassword] = React.useState("")
  const [formBirthday, setFormBirthday] = React.useState("")

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try { const res = await customerService.getAll(); setData(res) }
    catch { toast.error("Failed to load customers") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const resetForm = () => { setFormName(""); setFormPhone(""); setFormEmail(""); setFormUsername(""); setFormPassword(""); setFormBirthday(""); setEditing(null) }
  const openCreate = () => { resetForm(); setSheetOpen(true) }
  const openEdit = (item: Customer) => {
    setEditing(item); setFormName(item.name); setFormPhone(item.phone ?? ""); setFormEmail(item.email); setFormUsername(item.username); setFormPassword(""); setFormBirthday(item.birthday ? item.birthday.slice(0, 10) : ""); setSheetOpen(true)
  }

  const handleSubmit = async () => {
    if (!formName.trim()) { toast.error("Please enter a name"); return }
    setSubmitting(true)
    try {
      if (editing) {
        await customerService.update(editing.id, { name: formName.trim(), phone: formPhone || undefined, birthday: formBirthday || undefined } as UpdateCustomerAdminDto)
        toast.success("Customer updated successfully")
      } else {
        await customerService.create({ name: formName.trim(), phone: formPhone || undefined, username: formUsername || undefined, email: formEmail || undefined, password: formPassword || undefined, birthday: formBirthday || undefined } as CreateCustomerDto)
        toast.success("Customer added successfully")
      }
      setSheetOpen(false); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setSubmitting(false) }
  }

  const confirmDelete = (item: Customer) => { setDeleteTarget(item); setDeleteOpen(true) }
  const handleDelete = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try { await customerService.delete(deleteTarget.id); toast.success("Customer deleted successfully"); setDeleteOpen(false); loadData() }
    catch { toast.error("Failed to delete customer") }
    finally { setDeleting(false) }
  }

  const handleAddPoints = async () => {
    if (!pointsTarget || pointsAmount <= 0) { toast.error("Enter a valid points amount"); return }
    if (!pointsReason.trim()) { toast.error("Enter a reason"); return }
    setPointsSubmitting(true)
    try {
      await customerService.addPoints(pointsTarget.id, { points: pointsAmount, reason: pointsReason.trim() })
      toast.success(`Added ${pointsAmount} points to ${pointsTarget.name}`)
      setPointsOpen(false); setPointsAmount(0); setPointsReason(""); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setPointsSubmitting(false) }
  }

  const openOrderHistory = async (customer: Customer) => {
    setOrderHistoryTarget(customer)
    setOrderHistoryOpen(true)
    setOrderHistoryLoading(true)
    try {
      const orders = await customerService.getOrderHistory(customer.id)
      setOrderHistoryData(orders)
    } catch { toast.error("Failed to load order history") }
    finally { setOrderHistoryLoading(false) }
  }

  const columns: ColumnDef<Customer>[] = [
    {
      id: "qrCode",
      header: "QR",
      size: 60,
      cell: ({ row }) => (
        <Button variant="ghost" size="icon-sm" className="size-8" onClick={(e) => { e.stopPropagation(); setQrTarget(row.original) }}>
          <QrCode className="size-4" />
        </Button>
      ),
    },
    {
      id: "name",
      accessorKey: "name",
      header: "Name",
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          {row.original.avatarUrl ? <img src={row.original.avatarUrl} alt="" className="size-8 rounded-full object-cover" /> : <div className="flex size-8 items-center justify-center rounded-full bg-muted text-xs font-medium">{row.original.name.charAt(0)}</div>}
          <span>{row.original.name}</span>
        </div>
      ),
    },
    { id: "email", accessorKey: "email", header: "Email" },
    { id: "phone", accessorKey: "phone", header: "Phone", cell: ({ row }) => row.original.phone || "—" },
    { id: "customerCode", accessorKey: "customerCode", header: "Code" },
    { id: "loyaltyPoints", accessorKey: "loyaltyPoints", header: "Points" },
    { id: "membershipLevel", header: "Tier", cell: ({ row }) => row.original.membershipLevel ? <StatusBadge status={row.original.membershipLevel} /> : "—" },
    { id: "birthday", header: "Birthday", cell: ({ row }) => row.original.birthday ? formatDate(row.original.birthday) : "—" },
    { id: "createdAt", header: "Created", cell: ({ row }) => formatDateTime(row.original.createdAt) },
    {
      id: "actions",
      header: "",
      cell: ({ row }) => (
        <div className="flex justify-end gap-1">
          <Button variant="ghost" size="icon-sm" onClick={() => openOrderHistory(row.original)} title="Order history"><History className="size-4" /></Button>
          <Button variant="ghost" size="icon-sm" onClick={() => { setPointsTarget(row.original); setPointsOpen(true) }} title="Add points"><Coins className="size-4 text-green-600" /></Button>
          <Button variant="ghost" size="icon-sm" onClick={() => openEdit(row.original)}><Edit className="size-4" /></Button>
          <Button variant="ghost" size="icon-sm" onClick={() => confirmDelete(row.original)}><Trash2 className="size-4 text-destructive" /></Button>
        </div>
      ),
    },
  ]

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Customers</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <DataTable columns={columns} data={data} searchKey="name" searchPlaceholder="Search customers..." loading={loading}
          toolbarActions={<Button className="gap-1.5" onClick={openCreate}><Plus className="size-4" />Add Customer</Button>} />
      </div>
      <CrudSheet open={sheetOpen} onOpenChange={setSheetOpen} title={editing ? "Edit Customer" : "Add Customer"} onSubmit={handleSubmit} submitting={submitting}>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Full Name</Label>
            <Input id="name" value={formName} onChange={(e) => setFormName(e.target.value)} placeholder="John Doe" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input id="email" type="email" value={formEmail} onChange={(e) => setFormEmail(e.target.value)} placeholder="a@example.com" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="phone">Phone Number</Label>
            <Input id="phone" value={formPhone} onChange={(e) => setFormPhone(e.target.value)} placeholder="0123456789" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="birthday">Birthday</Label>
            <Input id="birthday" type="date" value={formBirthday} onChange={(e) => setFormBirthday(e.target.value)} />
          </div>
          {!editing && (
            <>
              <div className="space-y-2">
                <Label htmlFor="username">Username</Label>
                <Input id="username" value={formUsername} onChange={(e) => setFormUsername(e.target.value)} placeholder="Leave blank = auto-generate" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="password">Password</Label>
                <Input id="password" type="password" value={formPassword} onChange={(e) => setFormPassword(e.target.value)} placeholder="Leave blank = 123456" />
              </div>
            </>
          )}
        </div>
      </CrudSheet>
      <DeleteConfirmDialog open={deleteOpen} onOpenChange={setDeleteOpen} onConfirm={handleDelete} loading={deleting} itemName={deleteTarget?.name} />
      <Dialog open={!!qrTarget} onOpenChange={(open) => { if (!open) setQrTarget(null) }}>
        <DialogContent className="w-fit sm:max-w-sm" showCloseButton={true}>
          <DialogHeader>
            <DialogTitle>QR Code - {qrTarget?.name}</DialogTitle>
          </DialogHeader>
          <div className="flex flex-col items-center gap-3 py-4">
            {qrTarget && <QRCodeSVG value={qrTarget.customerCode} size={200} />}
            <p className="text-sm text-muted-foreground">Code: {qrTarget?.customerCode}</p>
          </div>
        </DialogContent>
      </Dialog>
      <Dialog open={pointsOpen} onOpenChange={(open) => { if (!open) { setPointsOpen(false); setPointsAmount(0); setPointsReason("") } }}>
        <DialogContent className="sm:max-w-sm">
          <DialogHeader><DialogTitle>Add Points - {pointsTarget?.name}</DialogTitle></DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Points</Label>
              <Input type="number" min="1" value={pointsAmount || ""} onChange={(e) => setPointsAmount(Number(e.target.value))} placeholder="Enter points amount" />
            </div>
            <div className="space-y-2">
              <Label>Reason</Label>
              <Input value={pointsReason} onChange={(e) => setPointsReason(e.target.value)} placeholder="e.g. Birthday reward, loyalty bonus..." />
            </div>
            <Button className="w-full" onClick={handleAddPoints} disabled={pointsSubmitting}>{pointsSubmitting ? "Processing..." : "Add Points"}</Button>
          </div>
        </DialogContent>
      </Dialog>

      <Sheet open={orderHistoryOpen} onOpenChange={(open) => { if (!open) { setOrderHistoryOpen(false); setOrderHistoryData([]) } }}>
        <SheetContent className="sm:max-w-xl">
          <SheetHeader><SheetTitle>Order History - {orderHistoryTarget?.name}</SheetTitle></SheetHeader>
          <div className="mt-4 space-y-2">
            {orderHistoryLoading ? (
              <p className="text-sm text-muted-foreground">Loading...</p>
            ) : orderHistoryData.length === 0 ? (
              <p className="text-sm text-muted-foreground">No orders yet</p>
            ) : (
              orderHistoryData.map((order) => (
                <div key={order.id} className="flex items-center justify-between rounded-lg border p-3">
                  <div>
                    <p className="text-sm font-medium">{order.orderCode || "—"}</p>
                    <p className="text-xs text-muted-foreground">{formatDateTime(order.createdAt)}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <Badge variant="outline">{order.status}</Badge>
                    <span className="text-sm font-semibold">{order.totalAmount ? formatCurrency(order.totalAmount) : "—"}</span>
                  </div>
                </div>
              ))
            )}
          </div>
        </SheetContent>
      </Sheet>
    </>
  )
}
