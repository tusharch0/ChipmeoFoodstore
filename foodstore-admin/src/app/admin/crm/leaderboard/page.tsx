"use client"

import * as React from "react"
import { Award, Crown, Medal, Star, Trophy } from "lucide-react"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Badge } from "@/components/ui/badge"
import { customerService } from "@/lib/services/customer-service"
import { cn } from "@/lib/utils"
import type { Customer } from "@/lib/types"

const rankColors = ["text-yellow-500", "text-gray-400", "text-amber-700"]
const rankIcons = [Trophy, Medal, Award]

const levelBadge: Record<string, string> = {
  bronze: "bg-amber-700/10 text-amber-700 border-amber-700/20",
  silver: "bg-gray-300/10 text-gray-400 border-gray-400/20",
  gold: "bg-yellow-500/10 text-yellow-500 border-yellow-500/20",
  platinum: "bg-blue-500/10 text-blue-500 border-blue-500/20",
  diamond: "bg-purple-500/10 text-purple-500 border-purple-500/20",
}

export default function LeaderboardPage() {
  const [data, setData] = React.useState<Customer[]>([])
  const [loading, setLoading] = React.useState(true)

  React.useEffect(() => {
    (async () => {
      try {
        const res = await customerService.getAll()
        setData(res.sort((a, b) => b.loyaltyPoints - a.loyaltyPoints))
      } catch { toast.error("Failed to load leaderboard") }
      finally { setLoading(false) }
    })()
  }, [])

  if (loading) {
    return (
      <div className="flex flex-1 items-center justify-center">
        <div className="size-8 animate-spin rounded-full border-4 border-muted border-t-primary" />
      </div>
    )
  }

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Loyalty Points Leaderboard</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="grid gap-3 md:grid-cols-3">
          {data.slice(0, 3).map((c, i) => {
            const Icon = rankIcons[i]
            return (
              <div key={c.id} className={cn("relative flex flex-col items-center gap-2 rounded-xl border p-6 text-center", i === 0 && "border-yellow-500/30 bg-yellow-500/5")}>
                <Icon className={cn("size-8", rankColors[i])} />
                {i === 0 && <Crown className="absolute -top-3 size-6 text-yellow-500" />}
                <div className="flex size-14 items-center justify-center rounded-full bg-muted text-xl font-bold">{c.name.charAt(0)}</div>
                <div>
                  <p className="font-semibold">{c.name}</p>
                  <p className="text-xs text-muted-foreground">{c.customerCode}</p>
                </div>
                <p className="text-2xl font-bold">{c.loyaltyPoints.toLocaleString()}</p>
                <p className="text-sm text-muted-foreground">points</p>
                {c.membershipLevel && (
                  <Badge variant="outline" className={levelBadge[c.membershipLevel.toLowerCase()] ?? ""}>{c.membershipLevel}</Badge>
                )}
              </div>
            )
          })}
        </div>

        <div className="rounded-lg border">
          <div className="p-4 text-sm font-medium text-muted-foreground">All Customers</div>
          <div className="divide-y">
            {data.map((c, i) => (
              <div key={c.id} className="flex items-center gap-4 px-4 py-3 hover:bg-muted/50">
                <span className={cn("flex size-8 shrink-0 items-center justify-center rounded-full text-sm font-bold", i < 3 ? (rankColors[i] + " bg-muted") : "text-muted-foreground")}>{i + 1}</span>
                <div className="flex size-10 items-center justify-center rounded-full bg-muted text-sm font-medium">{c.name.charAt(0)}</div>
                <div className="min-w-0 flex-1">
                  <p className="truncate font-medium">{c.name}</p>
                  <p className="truncate text-xs text-muted-foreground">{c.customerCode} · {c.email}</p>
                </div>
                {c.membershipLevel && (
                  <Badge variant="outline" className={cn("hidden sm:inline-flex", levelBadge[c.membershipLevel.toLowerCase()] ?? "")}>{c.membershipLevel}</Badge>
                )}
                <div className="text-right">
                  <p className="font-bold">{c.loyaltyPoints.toLocaleString()}</p>
                  <p className="text-xs text-muted-foreground">points</p>
                </div>
              </div>
            ))}
            {data.length === 0 && (
              <div className="flex flex-col items-center gap-2 py-12 text-muted-foreground">
                <Star className="size-8" />
                <span>No customers yet</span>
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  )
}
