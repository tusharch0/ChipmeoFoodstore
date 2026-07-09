"use client"

import * as React from "react"
import { Edit, Plus, Trash2 } from "lucide-react"
import type { ColumnDef } from "@tanstack/react-table"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Switch } from "@/components/ui/switch"
import { NativeSelect } from "@/components/ui/native-select"
import { DataTable } from "@/components/data-table"
import { CrudSheet } from "@/components/crud-sheet"
import { DeleteConfirmDialog } from "@/components/confirm-dialog"
import { StatusBadge } from "@/components/status-badge"
import { discountService } from "@/lib/services/discount-service"
import { formatCurrency, formatDateTime } from "@/lib/utils"
import type { Discount, DiscountCreateDto, DiscountUpdateDto } from "@/lib/types"

export default function DiscountsPage() {
  const [data, setData] = React.useState<Discount[]>([])
  const [loading, setLoading] = React.useState(true)
  const [sheetOpen, setSheetOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<Discount | null>(null)
  const [submitting, setSubmitting] = React.useState(false)
  const [deleteOpen, setDeleteOpen] = React.useState(false)
  const [deleting, setDeleting] = React.useState(false)
  const [deleteTarget, setDeleteTarget] = React.useState<Discount | null>(null)

  const [formCode, setFormCode] = React.useState("")
  const [formName, setFormName] = React.useState("")
  const [formDescription, setFormDescription] = React.useState("")
  const [formType, setFormType] = React.useState<"percent" | "amount">("percent")
  const [formValue, setFormValue] = React.useState(0)
  const [formMaxDiscount, setFormMaxDiscount] = React.useState<number | null>(null)
  const [formMinOrder, setFormMinOrder] = React.useState<number | null>(null)
  const [formUsageLimit, setFormUsageLimit] = React.useState<number | null>(null)
  const [formStartDate, setFormStartDate] = React.useState("")
  const [formEndDate, setFormEndDate] = React.useState("")
  const [formIsActive, setFormIsActive] = React.useState(true)

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try { const res = await discountService.getAll(); setData(res) }
    catch { toast.error("Failed to load discounts") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const resetForm = () => {
    setFormCode(""); setFormName(""); setFormDescription(""); setFormType("percent")
    setFormValue(0); setFormMaxDiscount(null); setFormMinOrder(null)
    setFormUsageLimit(null); setFormStartDate(""); setFormEndDate(""); setFormIsActive(true); setEditing(null)
  }

  const openCreate = () => { resetForm(); setSheetOpen(true) }

  const openEdit = (item: Discount) => {
    setEditing(item)
    setFormCode(item.code); setFormName(item.name); setFormDescription(item.description ?? "")
    setFormType(item.type); setFormValue(item.value)
    setFormMaxDiscount(item.maxDiscountAmount ?? null); setFormMinOrder(item.minOrderAmount ?? null)
    setFormUsageLimit(item.usageLimit ?? null)
    setFormStartDate(item.startDate ? item.startDate.slice(0, 10) : "")
    setFormEndDate(item.endDate ? item.endDate.slice(0, 10) : "")
    setFormIsActive(item.isActive); setSheetOpen(true)
  }

  const handleSubmit = async () => {
    if (!formCode.trim() || !formName.trim()) { toast.error("Please enter code and name"); return }
    if (formValue <= 0) { toast.error("Invalid value"); return }
    setSubmitting(true)
    try {
      const base = {
        code: formCode.trim(), name: formName.trim(), description: formDescription || undefined,
        type: formType, value: formValue,
        maxDiscountAmount: formMaxDiscount, minOrderAmount: formMinOrder, usageLimit: formUsageLimit,
        startDate: formStartDate ? new Date(formStartDate).toISOString() : null,
        endDate: formEndDate ? new Date(formEndDate).toISOString() : null, isActive: formIsActive,
      }
      if (editing) { await discountService.update(editing.id, base as DiscountUpdateDto); toast.success("Discount updated successfully") }
      else { await discountService.create(base as DiscountCreateDto); toast.success("Discount added successfully") }
      setSheetOpen(false); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setSubmitting(false) }
  }

  const confirmDelete = (item: Discount) => { setDeleteTarget(item); setDeleteOpen(true) }
  const handleDelete = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try { await discountService.delete(deleteTarget.id); toast.success("Discount deleted successfully"); setDeleteOpen(false); loadData() }
    catch { toast.error("Failed to delete discount") }
    finally { setDeleting(false) }
  }

  const columns: ColumnDef<Discount>[] = [
    { id: "code", accessorKey: "code", header: "Code" },
    { id: "name", accessorKey: "name", header: "Name" },
    { id: "type", header: "Type", cell: ({ row }) => row.original.type === "percent" ? "%" : "VND" },
    { id: "value", header: "Value", cell: ({ row }) => row.original.type === "percent" ? `${row.original.value}%` : formatCurrency(row.original.value) },
    { id: "usage", header: "Used", cell: ({ row }) => `${row.original.usedCount ?? 0}/${row.original.usageLimit ?? "∞"}` },
    { id: "startDate", header: "From", cell: ({ row }) => row.original.startDate ? formatDateTime(row.original.startDate) : "—" },
    { id: "endDate", header: "To", cell: ({ row }) => row.original.endDate ? formatDateTime(row.original.endDate) : "—" },
    { id: "isActive", header: "Status", cell: ({ row }) => <StatusBadge status={row.original.isActive} /> },
    { id: "actions", header: "", cell: ({ row }) => (
      <div className="flex justify-end gap-1">
        <Button variant="ghost" size="icon-sm" onClick={() => openEdit(row.original)}><Edit className="size-4" /></Button>
        <Button variant="ghost" size="icon-sm" onClick={() => confirmDelete(row.original)}><Trash2 className="size-4 text-destructive" /></Button>
      </div>
    )},
  ]

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Discount Codes</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <DataTable columns={columns} data={data} searchKey="name" searchPlaceholder="Search discounts..." loading={loading}
          toolbarActions={<Button className="gap-1.5" onClick={openCreate}><Plus className="size-4" />Add Discount</Button>} />
      </div>
      <CrudSheet open={sheetOpen} onOpenChange={setSheetOpen} title={editing ? "Edit Discount" : "Add Discount"} onSubmit={handleSubmit} submitting={submitting}>
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="code">Code</Label>
              <Input id="code" value={formCode} onChange={(e) => setFormCode(e.target.value)} placeholder="e.g. SUMMER50" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="name">Name</Label>
              <Input id="name" value={formName} onChange={(e) => setFormName(e.target.value)} placeholder="e.g. Summer Sale" />
            </div>
          </div>
          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Input id="description" value={formDescription} onChange={(e) => setFormDescription(e.target.value)} />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="type">Type</Label>
              <NativeSelect id="type" value={formType} onChange={(e) => setFormType(e.target.value as "percent" | "amount")}>
                <option value="percent">Percentage (%)</option>
                <option value="amount">Fixed Amount (VND)</option>
              </NativeSelect>
            </div>
            <div className="space-y-2">
              <Label htmlFor="value">Value</Label>
              <Input id="value" type="number" min={0} value={formValue} onChange={(e) => setFormValue(Number(e.target.value))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="maxDiscount">Max Discount</Label>
              <Input id="maxDiscount" type="number" min={0} value={formMaxDiscount ?? ""} onChange={(e) => setFormMaxDiscount(e.target.value ? Number(e.target.value) : null)} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="minOrder">Min Order</Label>
              <Input id="minOrder" type="number" min={0} value={formMinOrder ?? ""} onChange={(e) => setFormMinOrder(e.target.value ? Number(e.target.value) : null)} />
            </div>
          </div>
          <div className="space-y-2">
            <Label htmlFor="usageLimit">Usage Limit</Label>
            <Input id="usageLimit" type="number" min={0} value={formUsageLimit ?? ""} onChange={(e) => setFormUsageLimit(e.target.value ? Number(e.target.value) : null)} />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="startDate">Start Date</Label>
              <Input id="startDate" type="date" value={formStartDate} onChange={(e) => setFormStartDate(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="endDate">End Date</Label>
              <Input id="endDate" type="date" value={formEndDate} onChange={(e) => setFormEndDate(e.target.value)} />
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Switch id="isActive" checked={formIsActive} onCheckedChange={setFormIsActive} />
            <Label htmlFor="isActive">Active</Label>
          </div>
        </div>
      </CrudSheet>
      <DeleteConfirmDialog open={deleteOpen} onOpenChange={setDeleteOpen} onConfirm={handleDelete} loading={deleting} itemName={deleteTarget?.name} />
    </>
  )
}
