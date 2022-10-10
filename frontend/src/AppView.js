import {Container, Content, Header, Nav, Navbar} from "rsuite";
import React, {useState} from "react";
import Schemes from "./Schemes";
import Processes from "./Processes";
import Designer from "./Designer";

const navigationItems = [
    {name: 'Schemes', component: Schemes},
    {name: 'Processes', component: Processes},
    {name: 'Designer', component: Designer}
];

const AppView = () => {
    const [tab, setTab] = useState(navigationItems[0].name);
    const [schemeCode, setSchemeCode] = useState('Test1');
    const [processId, setProcessId] = useState();
    const items = navigationItems.map(
        item => <Nav.Item key={item.name} active={tab === item.name} onClick={() => setTab(item.name)}>{item.name}</Nav.Item>);
    const Child = navigationItems.find(item => item.name === tab)?.component
    const childProps = {
        onRowClick: (data) => {
            if (data.code) {
                setSchemeCode(data.code)
                setProcessId(undefined)
                setTab('Designer')
            } else if (data.id) {
                setSchemeCode(data.scheme);
                setProcessId(data.id);
                setTab('Designer')
            }
        },
        schemeCode: schemeCode,
        processId: processId
    }
    return <Container>
        <Header>
            <Navbar>
                <Nav>
                    {items}
                </Nav>
            </Navbar>
        </Header>
        <Content>
            <Child {...childProps}/>
        </Content>
    </Container>
}

export default AppView;
