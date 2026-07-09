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
import { DataTable } from "@/components/data-table"
import { CrudSheet } from "@/components/crud-sheet"
import { DeleteConfirmDialog } from "@/components/confirm-dialog"
import { StatusBadge } from "@/components/status-badge"
import { addonService } from "@/lib/services/addon-service"
import { formatCurrency } from "@/lib/utils"
import type { Addon, AddonCreateDto, AddonUpdateDto } from "@/lib/types"

export default function ToppingsPage() {
  const [data, setData] = React.useState<Addon[]>([])
  const [loading, setLoading] = React.useState(true)
  const [sheetOpen, setSheetOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<Addon | null>(null)
  const [submitting, setSubmitting] = React.useState(false)
  const [deleteOpen, setDeleteOpen] = React.useState(false)
  const [deleting, setDeleting] = React.useState(false)
  const [deleteTarget, setDeleteTarget] = React.useState<Addon | null>(null)

  const [formName, setFormName] = React.useState("")
  const [formPrice, setFormPrice] = React.useState(0)
  const [formIsActive, setFormIsActive] = React.useState(true)

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try { const res = await addonService.getAll(); setData(res) }
    catch { toast.error("Failed to load toppings") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const resetForm = () => { setFormName(""); setFormPrice(0); setFormIsActive(true); setEditing(null) }
  const openCreate = () => { resetForm(); setSheetOpen(true) }
  const openEdit = (item: Addon) => { setEditing(item); setFormName(item.name); setFormPrice(item.price); setFormIsActive(item.isActive); setSheetOpen(true) }

  const handleSubmit = async () => {
    if (!formName.trim()) { toast.error("Please enter a topping name"); return }
    if (formPrice < 0) { toast.error("Invalid price"); return }
    setSubmitting(true)
    try {
      if (editing) {
        await addonService.update(editing.id, { name: formName.trim(), price: formPrice, isActive: formIsActive } as AddonUpdateDto)
        toast.success("Topping updated successfully")
      } else {
        await addonService.create({ name: formName.trim(), price: formPrice, isActive: formIsActive } as AddonCreateDto)
        toast.success("Topping added successfully")
      }
      setSheetOpen(false); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setSubmitting(false) }
  }

  const confirmDelete = (item: Addon) => { setDeleteTarget(item); setDeleteOpen(true) }
  const handleDelete = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try { await addonService.delete(deleteTarget.id); toast.success("Topping deleted successfully"); setDeleteOpen(false); loadData() }
    catch { toast.error("Failed to delete topping") }
    finally { setDeleting(false) }
  }

  const columns: ColumnDef<Addon>[] = [
    { id: "name", accessorKey: "name", header: "Name" },
    { id: "price", accessorKey: "price", header: "Price", cell: ({ row }) => formatCurrency(row.original.price) },
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
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Topping</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <DataTable columns={columns} data={data} searchKey="name" searchPlaceholder="Search toppings..." loading={loading}
          toolbarActions={<Button className="gap-1.5" onClick={openCreate}><Plus className="size-4" />Add Topping</Button>} />
      </div>
      <CrudSheet open={sheetOpen} onOpenChange={setSheetOpen} title={editing ? "Edit Topping" : "Add Topping"} onSubmit={handleSubmit} submitting={submitting}>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Topping Name</Label>
            <Input id="name" value={formName} onChange={(e) => setFormName(e.target.value)} placeholder="e.g. Boba, Jelly..." />
          </div>
          <div className="space-y-2">
            <Label htmlFor="price">Price</Label>
            <Input id="price" type="number" min={0} value={formPrice} onChange={(e) => setFormPrice(Number(e.target.value))} />
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
