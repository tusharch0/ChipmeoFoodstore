"use client";
import * as React from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { Download, Landmark, RefreshCw } from "lucide-react";
import { toast } from "sonner";
import { DataTable } from "@/components/data-table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { SidebarTrigger } from "@/components/ui/sidebar";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbList,
  BreadcrumbPage,
} from "@/components/ui/breadcrumb";
import { financeService } from "@/lib/services/finance-service";
import type { LedgerJournal, Organization, WalletBalance } from "@/lib/types";
import { formatCurrency, formatDateTime } from "@/lib/utils";

export default function FinanceDashboardPage() {
  const [organizations, setOrganizations] = React.useState<Organization[]>([]);
  const [branchId, setBranchId] = React.useState("");
  const [wallet, setWallet] = React.useState<WalletBalance | null>(null);
  const [entries, setEntries] = React.useState<LedgerJournal[]>([]);
  const [loading, setLoading] = React.useState(true);
  const load = React.useCallback(
    async (branch?: string) => {
      const id = branch ?? branchId;
      if (!id) return;
      setLoading(true);
      try {
        const [w, l] = await Promise.all([
          financeService.wallet(id),
          financeService.ledger(id),
        ]);
        setWallet(w);
        setEntries(l);
      } catch (e) {
        toast.error((e as Error).message);
      } finally {
        setLoading(false);
      }
    },
    [branchId],
  );
  React.useEffect(() => {
    financeService
      .organizations()
      .then((items) => {
        setOrganizations(items);
        const first = items.flatMap((o) => o.branches).find((b) => b.isActive);
        if (first) {
          setBranchId(first.id);
          void load(first.id);
        }
      })
      .catch(() => toast.error("Failed to load branches"));
  }, [load]);
  const columns: ColumnDef<LedgerJournal>[] = [
    {
      accessorKey: "entryType",
      header: "Event",
      cell: ({ row }) => (
        <Badge variant="secondary">{row.original.entryType}</Badge>
      ),
    },
    { accessorKey: "description", header: "Description" },
    {
      id: "debits",
      header: "Debits",
      cell: ({ row }) => formatCurrency(row.original.debits),
    },
    {
      id: "credits",
      header: "Credits",
      cell: ({ row }) => formatCurrency(row.original.credits),
    },
    {
      id: "postedAt",
      header: "Posted",
      cell: ({ row }) => formatDateTime(row.original.postedAt),
    },
  ];
  const branches = organizations.flatMap((o) =>
    o.branches.map((b) => ({ ...b, organization: o.name })),
  );
  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb>
            <BreadcrumbList>
              <BreadcrumbItem>
                <BreadcrumbPage>Finance & Wallet</BreadcrumbPage>
              </BreadcrumbItem>
            </BreadcrumbList>
          </Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="flex items-end gap-3">
          <div className="grid gap-2">
            <Label htmlFor="branch">Restaurant branch</Label>
            <select
              id="branch"
              value={branchId}
              onChange={(e) => {
                setBranchId(e.target.value);
                void load(e.target.value);
              }}
              className="h-9 min-w-64 rounded-md border bg-background px-3 text-sm"
            >
              {branches.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.organization} — {b.name}
                </option>
              ))}
            </select>
          </div>
          <Button
            variant="outline"
            onClick={() => void load()}
            disabled={!branchId || loading}
          >
            <RefreshCw className="size-4" /> Refresh
          </Button>
        </div>
        <div className="grid gap-4 md:grid-cols-4">
          {[
            ["Available", wallet?.available],
            ["Pending", wallet?.pending],
            ["Held", wallet?.held],
            ["Wallet total", wallet?.total],
          ].map(([label, value]) => (
            <Card key={String(label)}>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  {label}
                </CardTitle>
              </CardHeader>
              <CardContent className="text-2xl font-bold">
                {value === undefined
                  ? "—"
                  : `${formatCurrency(value as number)} ${wallet?.currency ?? ""}`}
              </CardContent>
            </Card>
          ))}
        </div>
        <div>
          <Button
            variant="outline"
            onClick={() =>
              void financeService
                .exportLedger(branchId)
                .catch((error) => toast.error((error as Error).message))
            }
            disabled={!branchId}
          >
            <Download className="size-4" /> Statement CSV
          </Button>
        </div>
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Landmark className="size-5" /> Immutable journal
            </CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={columns}
              data={entries}
              loading={loading}
              searchKey="description"
              searchPlaceholder="Search journal entries..."
            />
          </CardContent>
        </Card>
      </div>
    </>
  );
}
