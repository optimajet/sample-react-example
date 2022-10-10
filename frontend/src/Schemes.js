import {Table} from "rsuite";
import {useEffect, useState} from "react";
import settings from "./settings";

const {Column, HeaderCell, Cell} = Table;

const Schemes = (props) => {
    const [data, setData] = useState([]);

    useEffect(() => {
        fetch(`${settings.workflowUrl}/schemes`)
            .then(response => response.json())
            .then(data => setData(data))
    }, []);

    return <Table data={data}
                  height={400}
                  onRowClick={rowData => props.onRowClick?.(rowData)}>
        <Column flexGrow={1}>
            <HeaderCell>Code</HeaderCell>
            <Cell dataKey="code"/>
        </Column>
        <Column flexGrow={1}>
            <HeaderCell>Tags</HeaderCell>
            <Cell dataKey="tags"/>
        </Column>
    </Table>
}

export default Schemes;
