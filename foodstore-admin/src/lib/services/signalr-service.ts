import * as signalR from "@microsoft/signalr";
import { getAuthToken } from "@/lib/auth-service";

type SignalREventHandler = (...args: unknown[]) => void;

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private handlers = new Map<string, SignalREventHandler[]>();

  async connect(hubUrl: string = "/api/proxy/hubs/app"): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;

    const branchId = typeof window === "undefined" ? null : window.localStorage.getItem("foodstore_admin_branch_id");
    const url = branchId ? `${hubUrl}?branchId=${encodeURIComponent(branchId)}` : hubUrl;
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: () => getAuthToken() ?? "" })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    this.handlers.forEach((handlerList, event) => {
      handlerList.forEach((handler) => {
        this.connection?.on(event, handler);
      });
    });

    await this.connection.start();
  }

  on(event: string, handler: SignalREventHandler): void {
    if (!this.handlers.has(event)) {
      this.handlers.set(event, []);
    }
    this.handlers.get(event)!.push(handler);
    this.connection?.on(event, handler);
  }

  off(event: string, handler: SignalREventHandler): void {
    const handlerList = this.handlers.get(event);
    if (handlerList) {
      const idx = handlerList.indexOf(handler);
      if (idx >= 0) handlerList.splice(idx, 1);
    }
    this.connection?.off(event, handler);
  }

  async invoke(method: string, ...args: unknown[]): Promise<unknown> {
    return this.connection?.invoke(method, ...args);
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }
}

export const signalRService = new SignalRService();
