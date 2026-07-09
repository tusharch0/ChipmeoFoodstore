"use client"

import * as React from "react"
import { Edit, Plus, Trash2 } from "lucide-react"
import Image from "next/image"
import type { ColumnDef } from "@tanstack/react-table"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Switch } from "@/components/ui/switch"
import { NativeSelect } from "@/components/ui/native-select"
import { DataTable } from "@/components/data-table"
import { CrudSheet } from "@/components/crud-sheet"
import { DeleteConfirmDialog } from "@/components/confirm-dialog"
import { StatusBadge } from "@/components/status-badge"
import { ImageUpload } from "@/components/image-upload"
import { comboService } from "@/lib/services/combo-service"
import { menuItemService } from "@/lib/services/menu-item-service"
import { mediaService } from "@/lib/services/media-service"
import { formatCurrency } from "@/lib/utils"
import type { Combo, ComboCreateDto, ComboUpdateDto, MenuItem, ComboItem } from "@/lib/types"

export default function CombosPage() {
  const [data, setData] = React.useState<Combo[]>([])
  const [menuItems, setMenuItems] = React.useState<MenuItem[]>([])
  const [loading, setLoading] = React.useState(true)
  const [sheetOpen, setSheetOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<Combo | null>(null)
  const [submitting, setSubmitting] = React.useState(false)
  const [deleteOpen, setDeleteOpen] = React.useState(false)
  const [deleting, setDeleting] = React.useState(false)
  const [deleteTarget, setDeleteTarget] = React.useState<Combo | null>(null)

  const [formName, setFormName] = React.useState("")
  const [formDescription, setFormDescription] = React.useState("")
  const [formComboPrice, setFormComboPrice] = React.useState(0)
  const [formImageUrl, setFormImageUrl] = React.useState<string | null>(null)
  const [formIsActive, setFormIsActive] = React.useState(true)
  const [formItems, setFormItems] = React.useState<ComboItem[]>([])

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try {
      const [combos, items] = await Promise.all([comboService.getAll(), menuItemService.getAll()])
      setData(combos)
      setMenuItems(items.filter((m) => m.isActive))
    } catch { toast.error("Failed to load data") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const resetForm = () => {
    setFormName(""); setFormDescription(""); setFormComboPrice(0)
    setFormImageUrl(null); setFormIsActive(true); setFormItems([]); setEditing(null)
  }

  const openCreate = () => { resetForm(); setSheetOpen(true) }

  const openEdit = (item: Combo) => {
    setEditing(item)
    setFormName(item.name); setFormDescription(item.description ?? "")
    setFormComboPrice(item.comboPrice); setFormImageUrl(item.imageUrl ?? null)
    setFormIsActive(item.isActive); setFormItems(item.items); setSheetOpen(true)
  }

  const addComboItem = () => {
    if (menuItems.length === 0) return
    setFormItems((prev) => [...prev, { menuItemId: menuItems[0].id, quantity: 1 }])
  }

  const updateComboItem = (index: number, field: keyof ComboItem, value: string | number) => {
    setFormItems((prev) => prev.map((item, i) => (i === index ? { ...item, [field]: value } : item)))
  }

  const removeComboItem = (index: number) => {
    setFormItems((prev) => prev.filter((_, i) => i !== index))
  }

  const handleSubmit = async () => {
    if (!formName.trim()) { toast.error("Please enter a combo name"); return }
    if (formComboPrice <= 0) { toast.error("Invalid price"); return }
    if (formItems.length === 0) { toast.error("Please add at least 1 item to the combo"); return }
    setSubmitting(true)
    try {
      const base = {
        name: formName.trim(), description: formDescription || undefined,
        comboPrice: formComboPrice, imageUrl: formImageUrl, isActive: formIsActive, items: formItems,
      }
      if (editing) { await comboService.update(editing.id, base as ComboUpdateDto); toast.success("Combo updated successfully") }
      else { await comboService.create(base as ComboCreateDto); toast.success("Combo added successfully") }
      setSheetOpen(false); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setSubmitting(false) }
  }

  const confirmDelete = (item: Combo) => { setDeleteTarget(item); setDeleteOpen(true) }
  const handleDelete = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try { await comboService.delete(deleteTarget.id); toast.success("Combo deleted successfully"); setDeleteOpen(false); loadData() }
    catch { toast.error("Failed to delete combo") }
    finally { setDeleting(false) }
  }

  const handleUpload = async (file: File) => {
    const result = await mediaService.upload(file, "combos")
    return result.fileUrl
  }

  const columns: ColumnDef<Combo>[] = [
    {
      id: "imageUrl",
      header: "Image",
      cell: ({ row }) => (
        row.original.imageUrl ? (
          <div className="relative size-10 overflow-hidden rounded-md">
            <Image src={row.original.imageUrl} alt="" fill className="object-cover" unoptimized />
          </div>
        ) : <div className="size-10 rounded-md bg-muted" />
      ),
    },
    { id: "name", accessorKey: "name", header: "Combo Name" },
    { id: "comboPrice", header: "Price", cell: ({ row }) => formatCurrency(row.original.comboPrice) },
    { id: "items", header: "Items", cell: ({ row }) => row.original.items.length },
    { id: "isActive", header: "Status", cell: ({ row }) => <StatusBadge status={row.original.isActive} /> },
    { id: "actions", header: "", cell: ({ row }) => (
      <div className="flex justify-end gap-1">
        <Button variant="ghost" size="icon-sm" onClick={() => openEdit(row.original)}><Edit className="size-4" /></Button>
        <Button variant="ghost" size="icon-sm" onClick={() => confirmDelete(row.original)}><Trash2 className="size-4 text-destructive" /></Button>
      </div>
    )},
  ]

  const getMenuItemName = (id: string) => menuItems.find((m) => m.id === id)?.name ?? `Item #${id.slice(0, 8)}`

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Combos</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <DataTable columns={columns} data={data} searchKey="name" searchPlaceholder="Search combos..." loading={loading}
          toolbarActions={<Button className="gap-1.5" onClick={openCreate}><Plus className="size-4" />Add Combo</Button>} />
      </div>
      <CrudSheet open={sheetOpen} onOpenChange={setSheetOpen} title={editing ? "Edit Combo" : "Add Combo"} onSubmit={handleSubmit} submitting={submitting}>
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="name">Combo Name</Label>
              <Input id="name" value={formName} onChange={(e) => setFormName(e.target.value)} placeholder="e.g. Family Combo" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="price">Combo Price</Label>
              <Input id="price" type="number" min={0} value={formComboPrice} onChange={(e) => setFormComboPrice(Number(e.target.value))} />
            </div>
          </div>
          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Textarea id="description" value={formDescription} onChange={(e) => setFormDescription(e.target.value)} />
          </div>
          <div className="space-y-2">
            <Label>Image</Label>
            <ImageUpload value={formImageUrl} onChange={setFormImageUrl} onUpload={handleUpload} />
          </div>
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <Label>Combo Items</Label>
              <Button type="button" variant="outline" size="sm" onClick={addComboItem}>+ Add Item</Button>
            </div>
            {formItems.map((item, index) => (
              <div key={index} className="flex items-end gap-2">
                <div className="flex-1 space-y-2">
                  <Label className="text-xs">Item</Label>
                  <NativeSelect value={item.menuItemId} onChange={(e) => updateComboItem(index, "menuItemId", e.target.value)}>
                    <option value="0">Select item</option>
                    {menuItems.filter((m) => m.isActive).map((m) => (
                      <option key={m.id} value={String(m.id)}>{m.name}</option>
                    ))}
                  </NativeSelect>
                </div>
                <div className="w-20 space-y-2">
                  <Label className="text-xs">Qty</Label>
                  <Input type="number" min={1} value={item.quantity} onChange={(e) => updateComboItem(index, "quantity", Number(e.target.value))} />
                </div>
                <Button variant="ghost" size="icon-sm" className="mb-0.5" onClick={() => removeComboItem(index)}>
                  <Trash2 className="size-4 text-destructive" />
                </Button>
              </div>
            ))}
            {formItems.length > 0 && (
              <div className="text-xs text-muted-foreground">
                Includes: {formItems.map((i) => `${getMenuItemName(i.menuItemId)} x${i.quantity}`).join(", ")}
              </div>
            )}
          </div>
          <div className="flex items-center gap-2">
            <Switch id="isActive" checked={formIsActive} onCheckedChange={setFormIsActive} />
            <Label htmlFor="isActive">Available</Label>
          </div>
        </div>
      </CrudSheet>
      <DeleteConfirmDialog open={deleteOpen} onOpenChange={setDeleteOpen} onConfirm={handleDelete} loading={deleting} itemName={deleteTarget?.name} />
    </>
  )
}
