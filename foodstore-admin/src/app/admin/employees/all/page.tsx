"use client"

import * as React from "react"
import { Edit, Plus, Trash2 } from "lucide-react"
import { toast } from "sonner"
import type { ColumnDef } from "@tanstack/react-table"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Switch } from "@/components/ui/switch"
import { DataTable } from "@/components/data-table"
import { CrudSheet } from "@/components/crud-sheet"
import { DeleteConfirmDialog } from "@/components/confirm-dialog"
import { ImageUpload } from "@/components/image-upload"
import { StatusBadge } from "@/components/status-badge"
import { NativeSelect, NativeSelectOption } from "@/components/ui/native-select"
import { employeeService } from "@/lib/services/employee-service"
import { mediaService } from "@/lib/services/media-service"
import { roleService } from "@/lib/services/role-service"
import { organizationService } from "@/lib/services/organization-service"
import { formatDateTime } from "@/lib/utils"
import type { Employee, EmployeeCreateDto, EmployeeUpdateDto, Organization, Role } from "@/lib/types"

export default function EmployeeAllPage() {
  const [data, setData] = React.useState<Employee[]>([])
  const [roles, setRoles] = React.useState<Role[]>([])
  const [organizations, setOrganizations] = React.useState<Organization[]>([])
  const [loading, setLoading] = React.useState(true)
  const [sheetOpen, setSheetOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<Employee | null>(null)
  const [submitting, setSubmitting] = React.useState(false)
  const [deleteOpen, setDeleteOpen] = React.useState(false)
  const [deleting, setDeleting] = React.useState(false)
  const [deleteTarget, setDeleteTarget] = React.useState<Employee | null>(null)

  const [formName, setFormName] = React.useState("")
  const [formUsername, setFormUsername] = React.useState("")
  const [formEmail, setFormEmail] = React.useState("")
  const [formPhone, setFormPhone] = React.useState("")
  const [formPassword, setFormPassword] = React.useState("")
  const [formRoleId, setFormRoleId] = React.useState("")
  const [formBranchId, setFormBranchId] = React.useState("")
  const [formIsActive, setFormIsActive] = React.useState(true)
  const [formImage, setFormImage] = React.useState<string | null>(null)

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try {
      const [emps, rls, orgs] = await Promise.all([employeeService.getAll(), roleService.getAll(), organizationService.list()])
      setData(emps); setRoles(rls.filter((r) => r.isActive)); setOrganizations(orgs)
    } catch { toast.error("Failed to load data") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const resetForm = () => { setFormName(""); setFormUsername(""); setFormEmail(""); setFormPhone(""); setFormPassword(""); setFormRoleId(""); setFormBranchId(organizations[0]?.branches[0]?.id ?? ""); setFormIsActive(true); setFormImage(null); setEditing(null) }
  const openCreate = () => { resetForm(); setSheetOpen(true) }
  const openEdit = (item: Employee) => {
    setEditing(item); setFormName(item.fullName); setFormUsername(item.username); setFormEmail(item.email ?? ""); setFormPhone(item.phone ?? ""); setFormPassword(""); setFormRoleId(item.roleId); setFormBranchId(item.branchId ?? ""); setFormIsActive(item.isActive); setFormImage(item.avatarUrl ?? null); setSheetOpen(true)
  }

  const handleUpload = async (file: File): Promise<string> => {
    const res = await mediaService.upload(file)
    return res.fileUrl
  }

  const handleSubmit = async () => {
    if (!formName.trim()) { toast.error("Please enter a name"); return }
    setSubmitting(true)
    try {
      const base = { fullName: formName.trim(), username: formUsername.trim(), email: formEmail || undefined, phone: formPhone || undefined, isActive: formIsActive, roleId: formRoleId, branchId: formBranchId, avatarUrl: formImage || undefined }
      if (editing) {
        await employeeService.update(editing.id, base as EmployeeUpdateDto)
        toast.success("Employee updated successfully")
      } else {
        await employeeService.create({ ...base, password: formPassword || "123456" } as EmployeeCreateDto)
        toast.success("Employee added successfully")
      }
      setSheetOpen(false); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setSubmitting(false) }
  }

  const confirmDelete = (item: Employee) => { setDeleteTarget(item); setDeleteOpen(true) }
  const handleDelete = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try { await employeeService.delete(deleteTarget.id); toast.success("Employee deleted successfully"); setDeleteOpen(false); loadData() }
    catch { toast.error("Failed to delete employee") }
    finally { setDeleting(false) }
  }

  const columns: ColumnDef<Employee>[] = [
    { id: "fullName", accessorKey: "fullName", header: "Name", cell: ({ row }) => (
      <div className="flex items-center gap-2">
        {row.original.avatarUrl ? <img src={row.original.avatarUrl} alt="" className="size-8 rounded-full object-cover" /> : <div className="flex size-8 items-center justify-center rounded-full bg-muted text-xs font-medium">{row.original.fullName.charAt(0)}</div>}
        <span>{row.original.fullName}</span>
      </div>
    )},
    { id: "email", accessorKey: "email", header: "Email", cell: ({ row }) => row.original.email || "—" },
    { id: "phone", accessorKey: "phone", header: "Phone", cell: ({ row }) => row.original.phone || "—" },
    { id: "role", header: "Role", cell: ({ row }) => row.original.roleName || "—" },
    { id: "branch", header: "Branch", cell: ({ row }) => row.original.branchName || "—" },
    { id: "isActive", header: "Status", cell: ({ row }) => <StatusBadge status={row.original.isActive} /> },
    { id: "createdAt", header: "Created", cell: ({ row }) => formatDateTime(row.original.createdAt) },
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
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Employees</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <DataTable columns={columns} data={data} searchKey="fullName" searchPlaceholder="Search employees..." loading={loading}
          toolbarActions={<Button className="gap-1.5" onClick={openCreate}><Plus className="size-4" />Add Employee</Button>} />
      </div>
      <CrudSheet open={sheetOpen} onOpenChange={setSheetOpen} title={editing ? "Edit Employee" : "Add Employee"} onSubmit={handleSubmit} submitting={submitting}>
        <div className="space-y-4">
          <ImageUpload value={formImage} onChange={setFormImage} onUpload={handleUpload} />
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
          {!editing && (
            <><div className="space-y-2">
              <Label htmlFor="username">Username</Label>
              <Input id="username" value={formUsername} onChange={(e) => setFormUsername(e.target.value)} />
            </div><div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <Input id="password" type="password" value={formPassword} onChange={(e) => setFormPassword(e.target.value)} placeholder="Leave blank = 123456" />
            </div></>
          )}
          <div className="space-y-2">
            <Label htmlFor="branchId">Branch</Label>
            <NativeSelect id="branchId" value={formBranchId} onChange={(e) => setFormBranchId(e.target.value)}>
              <NativeSelectOption value="">Select branch</NativeSelectOption>
              {organizations.flatMap((organization) => organization.branches.map((branch) => <NativeSelectOption key={branch.id} value={branch.id}>{organization.name} — {branch.name}</NativeSelectOption>))}
            </NativeSelect>
          </div>
          <div className="space-y-2">
            <Label htmlFor="roleId">Role</Label>
            <NativeSelect id="roleId" value={formRoleId} onChange={(e) => setFormRoleId(e.target.value)}>
              <NativeSelectOption value="">Select role</NativeSelectOption>
              {roles.map((r) => <NativeSelectOption key={r.id} value={r.id}>{r.name}</NativeSelectOption>)}
            </NativeSelect>
          </div>
          <div className="flex items-center gap-2">
            <Switch id="isActive" checked={formIsActive} onCheckedChange={setFormIsActive} />
            <Label htmlFor="isActive">Active</Label>
          </div>
        </div>
      </CrudSheet>
      <DeleteConfirmDialog open={deleteOpen} onOpenChange={setDeleteOpen} onConfirm={handleDelete} loading={deleting} itemName={deleteTarget?.fullName} />
    </>
  )
}
