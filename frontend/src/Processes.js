import {Table} from "rsuite";
import {useEffect, useState} from "react";
import settings from "./settings";

const {Column, HeaderCell, Cell} = Table;

const Processes = (props) => {
    const [data, setData] = useState([]);

    useEffect(() => {
        fetch(`${settings.workflowUrl}/instances`)
            .then(response => response.json())
            .then(data => setData(data))
    }, []);

    return <Table data={data}
                  height={400}
                  onRowClick={rowData => props.onRowClick?.(rowData)}>
        <Column flexGrow={1}>
            <HeaderCell>Id</HeaderCell>
            <Cell dataKey="id"/>
        </Column>
        <Column flexGrow={1}>
            <HeaderCell>Scheme</HeaderCell>
            <Cell dataKey="scheme"/>
        </Column>
        <Column flexGrow={1}>
            <HeaderCell>CreationDate</HeaderCell>
            <Cell dataKey="creationDate"/>
        </Column>
        <Column flexGrow={1}>
            <HeaderCell>StateName</HeaderCell>
            <Cell dataKey="stateName"/>
        </Column>
        <Column flexGrow={1}>
            <HeaderCell>ActivityName</HeaderCell>
            <Cell dataKey="activityName"/>
        </Column>
    </Table>
}

export default Processes;
