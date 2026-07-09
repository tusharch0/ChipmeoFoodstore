"use client";

import * as React from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { Download, FileCheck2, Plus } from "lucide-react";
import { toast } from "sonner";

import { DataTable } from "@/components/data-table";
import { CrudSheet } from "@/components/crud-sheet";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
import { settlementService } from "@/lib/services/settlement-service";
import type { Organization, SettlementBatch } from "@/lib/types";
import { formatCurrency } from "@/lib/utils";

const nextState: Record<string, string> = {
  draft: "reviewed",
  reviewed: "approved",
  approved: "submitted",
  submitted: "paid",
  paid: "reconciled",
  partially_paid: "reconciled",
};

export default function SettlementsDashboardPage() {
  const [orgs, setOrgs] = React.useState<Organization[]>([]);
  const [orgId, setOrgId] = React.useState("");
  const [batches, setBatches] = React.useState<SettlementBatch[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [selected, setSelected] = React.useState<SettlementBatch | null>(null);
  const [open, setOpen] = React.useState(false);
  const [payoutRef, setPayoutRef] = React.useState("");

  const load = React.useCallback(
    async (id = orgId) => {
      if (!id) return;
      setLoading(true);
      try {
        setBatches(await settlementService.list(id));
      } catch (e) {
        toast.error((e as Error).message);
      } finally {
        setLoading(false);
      }
    },
    [orgId],
  );

  React.useEffect(() => {
    financeService
      .organizations()
      .then((data) => {
        setOrgs(data);
        if (data[0]) {
          setOrgId(data[0].id);
          void load(data[0].id);
        }
      })
      .catch(() => toast.error("Failed to load restaurants"));
  }, [load]);

  const generate = async () => {
    const org = orgs.find((o) => o.id === orgId);
    if (!org) return;

    try {
      await settlementService.generate(
        orgId,
        new Date().toISOString().slice(0, 10),
        org.currencyCode,
      );
      toast.success("Settlement draft generated");
      void load();
    } catch (e) {
      toast.error((e as Error).message);
    }
  };

  const detail = async (batch: SettlementBatch) => {
    try {
      setSelected(await settlementService.get(batch.id));
      setPayoutRef("");
      setOpen(true);
    } catch {
      toast.error("Failed to load settlement statement");
    }
  };

  const advance = async () => {
    if (!selected) return;
    const target = nextState[selected.status];
    if (!target) return;

    try {
      await settlementService.transition(
        selected.id,
        target,
        payoutRef || undefined,
      );
      toast.success(`Settlement ${target.replace("_", " ")}`);
      setOpen(false);
      void load();
    } catch (e) {
      toast.error((e as Error).message);
    }
  };

  const columns: ColumnDef<SettlementBatch>[] = [
    { accessorKey: "periodDate", header: "Period" },
    {
      id: "gross",
      header: "Gross",
      cell: ({ row }) => formatCurrency(row.original.grossSales),
    },
    {
      id: "commission",
      header: "Commission",
      cell: ({ row }) => formatCurrency(row.original.commissions),
    },
    {
      id: "net",
      header: "Net payout",
      cell: ({ row }) => formatCurrency(row.original.netAmount),
    },
    {
      id: "status",
      header: "Status",
      cell: ({ row }) => (
        <Badge
          variant={
            row.original.status === "failed" ? "destructive" : "secondary"
          }
        >
          {row.original.status}
        </Badge>
      ),
    },
    {
      id: "actions",
      header: "",
      cell: ({ row }) => (
        <Button variant="ghost" size="sm" onClick={() => detail(row.original)}>
          Statement
        </Button>
      ),
    },
  ];

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb>
            <BreadcrumbList>
              <BreadcrumbItem>
                <BreadcrumbPage>Settlements</BreadcrumbPage>
              </BreadcrumbItem>
            </BreadcrumbList>
          </Breadcrumb>
        </div>
      </header>

      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="flex items-end gap-3">
          <div className="grid gap-2">
            <Label>Restaurant</Label>
            <select
              value={orgId}
              onChange={(e) => {
                setOrgId(e.target.value);
                void load(e.target.value);
              }}
              className="h-9 min-w-64 rounded-md border bg-background px-3 text-sm"
            >
              {orgs.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.name}
                </option>
              ))}
            </select>
          </div>
          <Button onClick={generate} disabled={!orgId}>
            <Plus className="size-4" />
            Generate daily draft
          </Button>
        </div>

        <DataTable
          columns={columns}
          data={batches}
          loading={loading}
          searchKey="periodDate"
          searchPlaceholder="Search settlement periods..."
        />
      </div>

      <CrudSheet
        open={open}
        onOpenChange={setOpen}
        title="Settlement statement"
        onSubmit={advance}
        submitLabel={
          selected && nextState[selected.status]
            ? `Mark ${nextState[selected.status].replace("_", " ")}`
            : "No action"
        }
      >
        <div className="space-y-4">
          <div className="flex items-center justify-between gap-3">
            <Badge variant="secondary">{selected?.status ?? "draft"}</Badge>
            {selected && (
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() =>
                  void settlementService
                    .export(selected.id)
                    .catch((error) => toast.error((error as Error).message))
                }
              >
                <Download className="size-4" />
                CSV
              </Button>
            )}
          </div>

          <div className="grid grid-cols-2 gap-2 text-sm">
            <span>Gross sales</span>
            <strong>{formatCurrency(selected?.grossSales ?? 0)}</strong>
            <span>Commission</span>
            <strong>{formatCurrency(selected?.commissions ?? 0)}</strong>
            <span>Provider fees</span>
            <strong>{formatCurrency(selected?.providerFees ?? 0)}</strong>
            <span>Net payout</span>
            <strong>{formatCurrency(selected?.netAmount ?? 0)}</strong>
          </div>

          {nextState[selected?.status ?? ""] === "paid" && (
            <div className="grid gap-2">
              <Label htmlFor="payoutReference">Payout reference</Label>
              <Input
                id="payoutReference"
                value={payoutRef}
                onChange={(e) => setPayoutRef(e.target.value)}
                placeholder="Bank transfer / payout reference"
              />
            </div>
          )}

          <div className="space-y-2">
            <Label>Transaction detail</Label>
            {selected?.lines?.map((line) => (
              <div className="rounded border p-2 text-xs" key={line.id}>
                <FileCheck2 className="mr-1 inline size-3" />
                {line.paymentIntentId}
                {line.providerReference ? ` / ${line.providerReference}` : ""} -
                gross {formatCurrency(line.grossAmount)}, net{" "}
                {formatCurrency(line.netAmount)}
              </div>
            ))}
          </div>
        </div>
      </CrudSheet>
    </>
  );
}
