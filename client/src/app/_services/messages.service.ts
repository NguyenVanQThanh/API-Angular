import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { setPaginationHeaders, getPaginationResult } from './paginationHelper';
import { Message } from '../_models/Message';
import { User } from '../_models/user';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { Group } from '../_models/group';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubsUrl;
  messageThread = signal<Message[]>([]);
  hubConnection?: HubConnection;
  constructor(private http: HttpClient) { }
  createHubConnection(user: User, otherUsername: string){
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();
    this.hubConnection.start().catch(error => console.log(error));
    this.hubConnection.on('ReceiveMessageThread', messages => {
      this.messageThread.set(messages)
    })
    this.hubConnection.on('NewMessage', message => {
      this.messageThread.update(messages => [...messages, message])
    })
    this.hubConnection.on('UpdatedGroup', (group: Group)=> {
      if (group.connections.some(x=> x.username === otherUsername)){
        this.messageThread.update(messages => {
          messages.forEach(message => {
            if (!message.dateRead){
              message.dateRead = new Date(Date.now());
            }
          })
          return messages;
        }
      )
      }
    })
  }
  stopHubConnection(){
    if (this.hubConnection?.state === HubConnectionState.Connected){
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }
  getMessages(pageNumber: number, pageSize: number,container: string ){
    let params = setPaginationHeaders(pageNumber,pageSize);
    params = params.append('Container', container);
    return getPaginationResult<Message[]>(this.baseUrl + 'message', params, this.http);
  }
  getMessageThread(username: string){
    return this.http.get<Message[]>(this.baseUrl + 'message/thread/' + username);
  }
  async sendMessage(username: string, content: string ){
    // return this.http.post<Message>(this.baseUrl + 'message', {recipientUsername: username, content });
    return this.hubConnection?.invoke('SendMessage', {recipientUsername: username, content});
  }
  deleteMessage(id: number){
    return this.http.delete(this.baseUrl + 'message/' + id);
  }
}