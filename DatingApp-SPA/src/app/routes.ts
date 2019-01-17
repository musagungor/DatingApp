import { AuthGuard } from './_guards/auth.guard';
import { MessagesComponent } from './messages/messages.component';
import { MemberListComponent } from './member-list/member-list.component';
import { ListsComponent } from './lists/lists.component';

import {Routes} from '@angular/router';
import { HomeComponent } from './home/home.component';

export const appRoutes: Routes = [
    {path: '', component: HomeComponent },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard],
        children: [
            {path: 'members', component: MemberListComponent },
            {path: 'list', component: ListsComponent },
            {path: 'messages', component: MessagesComponent }
        ]
    },
    {path: '**', redirectTo: '', pathMatch: 'full' }
];
