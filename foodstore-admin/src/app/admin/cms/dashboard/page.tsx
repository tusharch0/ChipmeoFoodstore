"use client"

import * as React from "react"
import { FileText, Eye, Edit, Clock, Calendar } from "lucide-react"
import { toast } from "sonner"
import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { blogService } from "@/lib/services/blog-service"
import { blogCategoryService } from "@/lib/services/blog-category-service"
import { blogTagService } from "@/lib/services/blog-tag-service"
import type { CmsDashboardStats } from "@/lib/types"

export default function CmsDashboard() {
  const [stats, setStats] = React.useState<CmsDashboardStats | null>(null)
  const [categoryCount, setCategoryCount] = React.useState(0)
  const [tagCount, setTagCount] = React.useState(0)

  React.useEffect(() => {
    (async () => {
      try {
        const [s, cats, tags] = await Promise.all([
          blogService.getDashboardStats(),
          blogCategoryService.getAll(),
          blogTagService.getAll(),
        ])
        setStats(s)
        setCategoryCount(cats.length)
        setTagCount(tags.length)
      } catch {
        toast.error("Failed to load CMS stats")
      }
    })()
  }, [])

  const cards = [
    { label: "Total Posts", value: stats?.totalPosts ?? "…", icon: FileText },
    { label: "Published", value: stats?.publishedPosts ?? "…", icon: Eye },
    { label: "Drafts", value: stats?.draftPosts ?? "…", icon: Edit },
    { label: "Scheduled", value: stats?.scheduledPosts ?? "…", icon: Clock },
    { label: "Categories", value: categoryCount, icon: FileText },
    { label: "Tags", value: tagCount, icon: FileText },
    { label: "Total Views", value: stats?.totalViews?.toLocaleString() ?? "…", icon: Eye },
    { label: "Featured Posts", value: stats?.featuredPostsCount ?? "…", icon: Calendar },
  ]

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Blog Overview</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="grid auto-rows-min gap-4 md:grid-cols-4">
          {cards.map((card) => (
            <div key={card.label} className="flex items-center gap-4 rounded-xl border p-4">
              <div className="flex size-12 items-center justify-center rounded-lg bg-primary/10">
                <card.icon className="size-6 text-primary" />
              </div>
              <div>
                <p className="text-2xl font-bold">{card.value}</p>
                <p className="text-sm text-muted-foreground">{card.label}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </>
  )
}