"use client"

import * as React from "react"
import { FileText, FileCheck, XCircle, Ban, Building2 } from "lucide-react"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { eInvoiceService } from "@/lib/services/e-invoice-service"
import type { EInvoiceDashboard } from "@/lib/types"

export default function EInvoiceDashboardPage() {
  const [data, setData] = React.useState<EInvoiceDashboard | null>(null)
  const [loading, setLoading] = React.useState(true)

  React.useEffect(() => {
    eInvoiceService.getDashboard()
      .then(setData)
      .catch(() => toast.error("Failed to load e-invoice stats"))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="size-8 animate-spin rounded-full border-4 border-muted border-t-primary" />
    </div>
  )

  const cards = [
    { title: "Total Invoices", value: data?.totalInvoices ?? 0, icon: FileText, color: "text-blue-600" },
    { title: "Issued", value: data?.issuedInvoices ?? 0, icon: FileCheck, color: "text-green-600" },
    { title: "Draft", value: data?.draftInvoices ?? 0, icon: FileText, color: "text-yellow-600" },
    { title: "Failed", value: data?.failedInvoices ?? 0, icon: XCircle, color: "text-red-600" },
    { title: "Cancelled", value: data?.cancelledInvoices ?? 0, icon: Ban, color: "text-gray-600" },
    { title: "Providers", value: data?.activeProviders ?? 0, icon: Building2, color: "text-purple-600" },
  ]

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>E-Invoice</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {cards.map(c => (
            <Card key={c.title}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">{c.title}</CardTitle>
                <c.icon className={`size-4 ${c.color}`} />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{c.value}</div>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </>
  )
}
