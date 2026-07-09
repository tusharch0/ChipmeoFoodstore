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
import { menuItemService } from "@/lib/services/menu-item-service"
import { categoryService } from "@/lib/services/category-service"
import { addonService } from "@/lib/services/addon-service"
import { mediaService } from "@/lib/services/media-service"
import { formatCurrency } from "@/lib/utils"
import type { MenuItem, MenuItemCreateDto, MenuItemUpdateDto, Category, Addon } from "@/lib/types"

export default function ProductsPage() {
  const [data, setData] = React.useState<MenuItem[]>([])
  const [categories, setCategories] = React.useState<Category[]>([])
  const [addons, setAddons] = React.useState<Addon[]>([])
  const [loading, setLoading] = React.useState(true)
  const [sheetOpen, setSheetOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<MenuItem | null>(null)
  const [submitting, setSubmitting] = React.useState(false)
  const [deleteOpen, setDeleteOpen] = React.useState(false)
  const [deleting, setDeleting] = React.useState(false)
  const [deleteTarget, setDeleteTarget] = React.useState<MenuItem | null>(null)

  const [formName, setFormName] = React.useState("")
  const [formDescription, setFormDescription] = React.useState("")
  const [formPrice, setFormPrice] = React.useState(0)
  const [formCategoryId, setFormCategoryId] = React.useState("")
  const [formImageUrl, setFormImageUrl] = React.useState<string | null>(null)
  const [formIsActive, setFormIsActive] = React.useState(true)
  const [formAddonIds, setFormAddonIds] = React.useState<string[]>([])

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try {
      const [items, cats, adds] = await Promise.all([
        menuItemService.getAll(),
        categoryService.getAll(),
        addonService.getAll(),
      ])
      setData(items)
      setCategories(cats)
      setAddons(adds)
    } catch { toast.error("Failed to load data") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const resetForm = () => {
    setFormName(""); setFormDescription(""); setFormPrice(0); setFormCategoryId("")
    setFormImageUrl(null); setFormIsActive(true); setFormAddonIds([]); setEditing(null)
  }

  const openCreate = () => { resetForm(); setSheetOpen(true) }

  const openEdit = (item: MenuItem) => {
    setEditing(item)
    setFormName(item.name); setFormDescription(item.description ?? "")
    setFormPrice(item.price); setFormCategoryId(item.categoryId)
    setFormImageUrl(item.imageUrl ?? null); setFormIsActive(item.isActive)
    setFormAddonIds(item.addons?.map((a) => a.id) ?? [])
    setSheetOpen(true)
  }

  const toggleAddon = (addonId: string) => {
    setFormAddonIds((prev) =>
      prev.includes(addonId) ? prev.filter((id) => id !== addonId) : [...prev, addonId]
    )
  }

  const handleSubmit = async () => {
    if (!formName.trim()) { toast.error("Please enter a name"); return }
    if (formPrice <= 0) { toast.error("Invalid price"); return }
    if (!formCategoryId) { toast.error("Please select a category"); return }
    setSubmitting(true)
    try {
      const base = {
        name: formName.trim(), description: formDescription || undefined,
        price: formPrice, categoryId: formCategoryId,
        imageUrl: formImageUrl, isActive: formIsActive, addonIds: formAddonIds,
      }
      if (editing) {
        await menuItemService.update(editing.id, base as MenuItemUpdateDto)
        toast.success("Product updated successfully")
      } else {
        await menuItemService.create(base as MenuItemCreateDto)
        toast.success("Product added successfully")
      }
      setSheetOpen(false); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setSubmitting(false) }
  }

  const confirmDelete = (item: MenuItem) => { setDeleteTarget(item); setDeleteOpen(true) }
  const handleDelete = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try { await menuItemService.delete(deleteTarget.id); toast.success("Product deleted successfully"); setDeleteOpen(false); loadData() }
    catch { toast.error("Failed to delete product") }
    finally { setDeleting(false) }
  }

  const handleUpload = async (file: File) => {
    const result = await mediaService.upload(file, "menu-items")
    return result.fileUrl
  }

  const columns: ColumnDef<MenuItem>[] = [
    {
      id: "avatar",
      header: "Image",
      cell: ({ row }) => (
        row.original.imageUrl ? (
          <div className="relative size-10 overflow-hidden rounded-md">
            <Image src={row.original.imageUrl} alt="" fill className="object-cover" unoptimized />
          </div>
        ) : <div className="size-10 rounded-md bg-muted" />
      ),
    },
    { id: "name", accessorKey: "name", header: "Product Name" },
    { id: "categoryName", accessorKey: "categoryName", header: "Category", cell: ({ row }) => row.original.categoryName ?? "—" },
    { id: "price", accessorKey: "price", header: "Price", cell: ({ row }) => formatCurrency(row.original.price) },
    {
      id: "isActive",
      header: "Status",
      cell: ({ row }) => <StatusBadge status={row.original.isActive} />,
    },
    {
      id: "actions",
      header: "",
      cell: ({ row }) => (
        <div className="flex justify-end gap-1">
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
          <Breadcrumb>
            <BreadcrumbList>
              <BreadcrumbItem><BreadcrumbPage>Products</BreadcrumbPage></BreadcrumbItem>
            </BreadcrumbList>
          </Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <DataTable columns={columns} data={data} searchKey="name" searchPlaceholder="Search products..." loading={loading}
          toolbarActions={<Button className="gap-1.5" onClick={openCreate}><Plus className="size-4" />Add Product</Button>} />
      </div>
      <CrudSheet open={sheetOpen} onOpenChange={setSheetOpen} title={editing ? "Edit Product" : "Add Product"} onSubmit={handleSubmit} submitting={submitting}>
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="name">Product Name</Label>
              <Input id="name" value={formName} onChange={(e) => setFormName(e.target.value)} placeholder="e.g. Beef Pho" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="price">Price</Label>
              <Input id="price" type="number" min={0} value={formPrice} onChange={(e) => setFormPrice(Number(e.target.value))} />
            </div>
          </div>
          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Textarea id="description" value={formDescription} onChange={(e) => setFormDescription(e.target.value)} placeholder="Product description..." />
          </div>
          <div className="space-y-2">
            <Label htmlFor="categoryId">Category</Label>
            <NativeSelect id="categoryId" value={formCategoryId} onChange={(e) => setFormCategoryId(e.target.value)}>
              <option value="">Select category</option>
              {categories.filter((c) => c.isActive).map((c) => (
                <option key={c.id} value={String(c.id)}>{c.name}</option>
              ))}
            </NativeSelect>
          </div>
          <div className="space-y-2">
            <Label>Image</Label>
            <ImageUpload value={formImageUrl} onChange={setFormImageUrl} onUpload={handleUpload} />
          </div>
          <div className="space-y-2">
            <Label>Toppings</Label>
            <div className="grid grid-cols-2 gap-2">
              {addons.filter((a) => a.isActive).map((addon) => (
                <label key={addon.id} className="flex items-center gap-2 rounded-lg border p-2 cursor-pointer hover:bg-muted">
                  <input type="checkbox" checked={formAddonIds.includes(addon.id)} onChange={() => toggleAddon(addon.id)} className="size-4" />
                  <div className="flex-1 text-sm">{addon.name}</div>
                  <div className="text-xs text-muted-foreground">+{formatCurrency(addon.price)}</div>
                </label>
              ))}
            </div>
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
