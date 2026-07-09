"use client";

import * as React from "react";
import { Check, Plus } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { CrudSheet } from "@/components/crud-sheet";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { SidebarTrigger } from "@/components/ui/sidebar";
import { Separator } from "@/components/ui/separator";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbList,
  BreadcrumbPage,
} from "@/components/ui/breadcrumb";
import { financeService } from "@/lib/services/finance-service";
import { financeOperationsService } from "@/lib/services/finance-operations-service";
import type {
  FinanceAdjustment,
  FinanceException,
  LedgerAccount,
  Organization,
  ReconciliationCase,
} from "@/lib/types";
import { formatCurrency } from "@/lib/utils";

export default function FinanceOperationsPage() {
  const [orgs, setOrgs] = React.useState<Organization[]>([]);
  const [orgId, setOrgId] = React.useState("");
  const [accounts, setAccounts] = React.useState<LedgerAccount[]>([]);
  const [adjustments, setAdjustments] = React.useState<FinanceAdjustment[]>([]);
  const [cases, setCases] = React.useState<ReconciliationCase[]>([]);
  const [exceptions, setExceptions] = React.useState<FinanceException[]>([]);
  const [open, setOpen] = React.useState(false);
  const [mode, setMode] = React.useState<"adjustment" | "case">("adjustment");
  const [form, setForm] = React.useState<Record<string, string>>({});

  const load = React.useCallback(
    async (id = orgId) => {
      if (!id) return;

      try {
        const [accountData, adjustmentData, caseData, exceptionData] =
          await Promise.all([
            financeService.accounts(id),
            financeOperationsService.adjustments(id),
            financeOperationsService.cases(id),
            financeOperationsService.exceptions(id),
          ]);
        setAccounts(accountData);
        setAdjustments(adjustmentData);
        setCases(caseData);
        setExceptions(exceptionData);
        financeOperationsService
          .unmatchedPayments()
          .then((items) => setExceptions((current) => [...items, ...current]))
          .catch(() => undefined);
      } catch (e) {
        toast.error((e as Error).message);
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

  const openAdjustment = () => {
    setMode("adjustment");
    setForm({
      ledgerAccountId: accounts[0]?.id ?? "",
      currency: accounts[0]?.currency ?? "KES",
    });
    setOpen(true);
  };

  const openCase = () => {
    setMode("case");
    setForm({ currency: accounts[0]?.currency ?? "KES" });
    setOpen(true);
  };

  const submit = async () => {
    try {
      if (mode === "adjustment") {
        await financeOperationsService.requestAdjustment({
          organizationId: orgId,
          ledgerAccountId: form.ledgerAccountId,
          amount: Number(form.amount),
          currency: form.currency || "KES",
          reasonCode: form.reasonCode,
          reason: form.reason,
        });
      } else {
        await financeOperationsService.createCase({
          organizationId: orgId,
          sourceType: form.sourceType,
          sourceReference: form.sourceReference,
          internalAmount: Number(form.internalAmount),
          externalAmount: Number(form.externalAmount),
          currency: form.currency || "KES",
          evidenceUrl: form.evidenceUrl,
        });
      }
      toast.success("Finance operation recorded");
      setOpen(false);
      setForm({});
      void load();
    } catch (e) {
      toast.error((e as Error).message);
    }
  };

  return (
    <>
      <header className="flex h-16 items-center gap-2">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger />
          <Separator orientation="vertical" className="h-4" />
          <Breadcrumb>
            <BreadcrumbList>
              <BreadcrumbItem>
                <BreadcrumbPage>Finance Operations</BreadcrumbPage>
              </BreadcrumbItem>
            </BreadcrumbList>
          </Breadcrumb>
        </div>
      </header>

      <div className="space-y-4 p-4 pt-0">
        <div className="flex items-end gap-3">
          <div className="grid gap-2">
            <Label>Restaurant</Label>
            <select
              className="h-9 rounded border px-3"
              value={orgId}
              onChange={(e) => {
                setOrgId(e.target.value);
                void load(e.target.value);
              }}
            >
              {orgs.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.name}
                </option>
              ))}
            </select>
          </div>
          <Button onClick={openAdjustment} disabled={!orgId}>
            <Plus />
            Adjustment
          </Button>
          <Button variant="outline" onClick={openCase} disabled={!orgId}>
            <Plus />
            Discrepancy
          </Button>
        </div>

        <div className="grid gap-4 lg:grid-cols-2">
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle>Operational exception queue</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-2 md:grid-cols-2">
              {exceptions.length === 0 && (
                <p className="text-sm text-muted-foreground">
                  No active financial exceptions.
                </p>
              )}
              {exceptions.map((item) => (
                <div
                  className="rounded border p-3"
                  key={`${item.type}-${item.id}`}
                >
                  <div className="flex items-center justify-between gap-2">
                    <Badge
                      variant={
                        item.type.includes("failure") ||
                        item.type === "negative_wallet" ||
                        item.type === "held_wallet"
                          ? "destructive"
                          : "secondary"
                      }
                    >
                      {item.type.replaceAll("_", " ")}
                    </Badge>
                    <span className="text-sm font-medium">
                      {item.amount === undefined
                        ? ""
                        : `${formatCurrency(item.amount)} ${item.currency}`}
                    </span>
                  </div>
                  <p className="mt-2 text-sm">{item.reference}</p>
                  {item.detail && (
                    <p className="text-xs text-muted-foreground">
                      {item.detail}
                    </p>
                  )}
                </div>
              ))}
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle>Adjustment approvals</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {adjustments.map((a) => (
                <div
                  className="flex items-center justify-between rounded border p-3"
                  key={a.id}
                >
                  <div>
                    <div className="font-medium">
                      {a.reasonCode}: {formatCurrency(a.amount)}
                    </div>
                    <div className="text-xs text-muted-foreground">
                      {a.reason}
                    </div>
                  </div>
                  {a.status === "pending" ? (
                    <Button
                      size="sm"
                      onClick={async () => {
                        try {
                          await financeOperationsService.approveAdjustment(
                            a.id,
                          );
                          toast.success("Adjustment approved");
                          void load();
                        } catch (e) {
                          toast.error((e as Error).message);
                        }
                      }}
                    >
                      <Check />
                      Approve
                    </Button>
                  ) : (
                    <Badge>{a.status}</Badge>
                  )}
                </div>
              ))}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Reconciliation cases</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {cases.map((c) => (
                <div
                  className="flex items-center justify-between rounded border p-3"
                  key={c.id}
                >
                  <div>
                    <div className="font-medium">
                      {c.sourceType}: {c.sourceReference}
                    </div>
                    <div className="text-xs text-muted-foreground">
                      Internal {formatCurrency(c.internalAmount)} - External{" "}
                      {formatCurrency(c.externalAmount)}
                    </div>
                  </div>
                  {c.status === "open" ? (
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={async () => {
                        const resolution = window.prompt("Resolution note");
                        if (!resolution) return;
                        try {
                          await financeOperationsService.resolveCase(
                            c.id,
                            resolution,
                          );
                          toast.success("Case resolved");
                          void load();
                        } catch (e) {
                          toast.error((e as Error).message);
                        }
                      }}
                    >
                      Resolve
                    </Button>
                  ) : (
                    <Badge>{c.status}</Badge>
                  )}
                </div>
              ))}
            </CardContent>
          </Card>
        </div>
      </div>

      <CrudSheet
        open={open}
        onOpenChange={setOpen}
        title={
          mode === "adjustment"
            ? "Request adjustment"
            : "Create reconciliation case"
        }
        onSubmit={submit}
        submitLabel="Submit"
      >
        <div className="space-y-3">
          {mode === "adjustment" ? (
            <>
              <Label>Ledger account</Label>
              <select
                className="h-9 w-full rounded-md border bg-background px-3 text-sm"
                value={form.ledgerAccountId || ""}
                onChange={(e) => {
                  const account = accounts.find((a) => a.id === e.target.value);
                  setForm({
                    ...form,
                    ledgerAccountId: e.target.value,
                    currency: account?.currency ?? form.currency ?? "KES",
                  });
                }}
              >
                {accounts.map((account) => (
                  <option key={account.id} value={account.id}>
                    {account.code} - {account.name} ({account.currency})
                  </option>
                ))}
              </select>
              <Label>Amount</Label>
              <Input
                type="number"
                value={form.amount || ""}
                onChange={(e) => setForm({ ...form, amount: e.target.value })}
              />
              <Label>Reason code</Label>
              <Input
                value={form.reasonCode || ""}
                onChange={(e) =>
                  setForm({ ...form, reasonCode: e.target.value })
                }
              />
              <Label>Reason</Label>
              <Input
                value={form.reason || ""}
                onChange={(e) => setForm({ ...form, reason: e.target.value })}
              />
            </>
          ) : (
            <>
              <Label>Source type</Label>
              <Input
                value={form.sourceType || ""}
                onChange={(e) =>
                  setForm({ ...form, sourceType: e.target.value })
                }
              />
              <Label>Reference</Label>
              <Input
                value={form.sourceReference || ""}
                onChange={(e) =>
                  setForm({ ...form, sourceReference: e.target.value })
                }
              />
              <Label>Internal amount</Label>
              <Input
                type="number"
                value={form.internalAmount || ""}
                onChange={(e) =>
                  setForm({ ...form, internalAmount: e.target.value })
                }
              />
              <Label>External amount</Label>
              <Input
                type="number"
                value={form.externalAmount || ""}
                onChange={(e) =>
                  setForm({ ...form, externalAmount: e.target.value })
                }
              />
            </>
          )}
          <Label>Currency</Label>
          <Input
            value={form.currency || "KES"}
            onChange={(e) => setForm({ ...form, currency: e.target.value })}
          />
        </div>
      </CrudSheet>
    </>
  );
}
