using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Nox.CCK.Network;
using Nox.CCK.Utils;
using Nox.Users;
using UnityEngine.Events;
using Logger = Nox.CCK.Utils.Logger;

namespace Nox.Table.Runtime {
	public class Network {
		private readonly UnityEvent<Entry> _getEvent = new();
		private readonly UnityEvent<Entry> _setEvent = new();
		private readonly UnityEvent<string, Identifier> _deleteEvent = new();

		private void InvokeGet(Entry entry) {
			if (entry == null)
				return;
			_getEvent.Invoke(entry);
			Main.Instance.CoreAPI.EventAPI.Emit("table_get", entry);
		}

		private void InvokeSet(Entry entry) {
			if (entry == null)
				return;
			_setEvent.Invoke(entry);
			Main.Instance.CoreAPI.EventAPI.Emit("table_set", entry);
			InvokeGet(entry);
		}

		private void InvokeDelete(string key, Identifier user) {
			_deleteEvent.Invoke(key, user);
			Main.Instance.CoreAPI.EventAPI.Emit("table_delete", key, user);
		}

		public async UniTask<Entry> Get(string key, CancellationToken cancellationToken = default) {

			var user = Main.UserAPI?.Current;
			if (user == null) {
				Logger.LogError($"Cannot delete table {key}: no user provided.");
				return null;
			}


			var request = await RequestNode.To(user.Server, $"/users/@me/tables/{Uri.EscapeDataString(key)}");
			if (request == null) {
				Logger.LogError($"Failed to find {user.Server} for table {key}");
				return null;
			}

			await request.Send(cancellationToken);
			if (!request.Ok()) {
				Logger.LogError($"Failed to get table {key} from {user.Server}");
				return null;
			}
			
			var response = new Entry(
				key: key,
				await request.Data(token: cancellationToken),
				mime: request.GetRequestHeader("Content-Type") ?? "application/octet-stream",
				user.Identifier
			);

			InvokeGet(response);
			return response;
		}

		public async UniTask<Entry> Set(
			string            key,
			string            value,
			string            mime              = "text/plain",
			CancellationToken cancellationToken = default
		)
			=> await Set(key, Encoding.UTF8.GetBytes(value), mime, cancellationToken);

		public async UniTask<Entry> Set(
			string            key,
			byte[]            value,
			string            mime              = "application/octet-stream",
			CancellationToken cancellationToken = default
		) {
			var user = Main.UserAPI?.Current;
			if (user == null) {
				Logger.LogError($"Cannot delete table {key}: no user provided.");
				return null;
			}

			var request = await RequestNode.To(user.Server, $"/users/@me/tables/{Uri.EscapeDataString(key)}");
			if (request == null) {
				Logger.LogError($"Failed to find {user.Server} for table {key}");
				return null;
			}
			request.SetRequestHeader("Content-Type", mime);
			request.SetBody(value);
			request.method = RequestExtension.Method.POST;
			await request.Send(cancellationToken);
			if (!request.Ok()) {
				Logger.LogError($"Failed to set table {key} on {user.Server}: {request.responseCode} {await request.Text(token: cancellationToken)}");
				return null;
			}

			var response = new Entry(key, value, mime, user.Identifier);

			InvokeSet(response);
			return response;
		}

		public async UniTask<bool> Delete(string key, CancellationToken cancellationToken = default) {
			var user = Main.UserAPI?.Current;
			if (user == null) {
				Logger.LogError($"Cannot delete table {key}: no user provided.");
				return false;
			}

			var request = await RequestNode.To(user.Server, $"/users/@me/tables/{Uri.EscapeDataString(key)}");
			if (request == null) {
				Logger.LogError($"Failed to find {user.Server} for table {key}");
				return false;
			}

			request.method = RequestExtension.Method.DELETE;
			await request.Send(cancellationToken);
			if (!request.Ok()) {
				Logger.LogError($"Failed to delete table {key} on {user.Server}");
				return false;
			}

			InvokeDelete(key, user.Identifier);
			return true;
		}
	}
}