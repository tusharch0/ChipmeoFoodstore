"use client"

import * as React from "react"
import { Edit, Plus, Trash2 } from "lucide-react"
import Image from "next/image"
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
import { ImageUpload } from "@/components/image-upload"
import { categoryService } from "@/lib/services/category-service"
import { mediaService } from "@/lib/services/media-service"
import type { Category, CategoryCreateDto, CategoryUpdateDto } from "@/lib/types"

export default function CategoriesPage() {
  const [data, setData] = React.useState<Category[]>([])
  const [loading, setLoading] = React.useState(true)
  const [sheetOpen, setSheetOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<Category | null>(null)
  const [submitting, setSubmitting] = React.useState(false)
  const [deleteOpen, setDeleteOpen] = React.useState(false)
  const [deleting, setDeleting] = React.useState(false)
  const [deleteTarget, setDeleteTarget] = React.useState<Category | null>(null)

  const [formName, setFormName] = React.useState("")
  const [formDescription, setFormDescription] = React.useState("")
  const [formImageUrl, setFormImageUrl] = React.useState<string | null>(null)
  const [formIsActive, setFormIsActive] = React.useState(true)

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try {
      const res = await categoryService.getAll()
      setData(res)
    } catch { toast.error("Failed to load categories") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const resetForm = () => {
    setFormName(""); setFormDescription(""); setFormImageUrl(null); setFormIsActive(true); setEditing(null)
  }

  const openCreate = () => { resetForm(); setSheetOpen(true) }

  const openEdit = (item: Category) => {
    setEditing(item)
    setFormName(item.name); setFormDescription(item.description ?? "")
    setFormImageUrl(item.imageUrl ?? null); setFormIsActive(item.isActive)
    setSheetOpen(true)
  }

  const handleSubmit = async () => {
    if (!formName.trim()) { toast.error("Please enter a category name"); return }
    setSubmitting(true)
    try {
      if (editing) {
        await categoryService.update(editing.id, { name: formName.trim(), description: formDescription || undefined, imageUrl: formImageUrl, isActive: formIsActive })
        toast.success("Category updated successfully")
      } else {
        await categoryService.create({ name: formName.trim(), description: formDescription || undefined, imageUrl: formImageUrl, isActive: formIsActive })
        toast.success("Category added successfully")
      }
      setSheetOpen(false); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setSubmitting(false) }
  }

  const confirmDelete = (item: Category) => { setDeleteTarget(item); setDeleteOpen(true) }
  const handleDelete = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try { await categoryService.delete(deleteTarget.id); toast.success("Category deleted successfully"); setDeleteOpen(false); loadData() }
    catch { toast.error("Failed to delete category") }
    finally { setDeleting(false) }
  }

  const handleUpload = async (file: File) => {
    const result = await mediaService.upload(file, "categories")
    return result.fileUrl
  }

  const columns: ColumnDef<Category>[] = [
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
    { id: "name", accessorKey: "name", header: "Name" },
    { id: "description", accessorKey: "description", header: "Description", cell: ({ row }) => row.original.description || "—" },
    { id: "isActive", header: "Status", cell: ({ row }) => <StatusBadge status={row.original.isActive} /> },
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
              <BreadcrumbItem><BreadcrumbPage>Categories</BreadcrumbPage></BreadcrumbItem>
            </BreadcrumbList>
          </Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <DataTable columns={columns} data={data} searchKey="name" searchPlaceholder="Search categories..." loading={loading}
          toolbarActions={<Button className="gap-1.5" onClick={openCreate}><Plus className="size-4" />Add Category</Button>} />
      </div>
      <CrudSheet open={sheetOpen} onOpenChange={setSheetOpen} title={editing ? "Edit Category" : "Add Category"} onSubmit={handleSubmit} submitting={submitting}>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Category Name</Label>
            <Input id="name" value={formName} onChange={(e) => setFormName(e.target.value)} placeholder="Enter category name" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Input id="description" value={formDescription} onChange={(e) => setFormDescription(e.target.value)} placeholder="Enter description" />
          </div>
          <div className="space-y-2">
            <Label>Image</Label>
            <ImageUpload value={formImageUrl} onChange={setFormImageUrl} onUpload={handleUpload} />
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
