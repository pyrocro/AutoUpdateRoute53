<h2>Description</h2>
A docker container to auto update "A records" (main domain name not subdomains) for a a given domain on Amazon's AWS Route53.

<b>My use case:</b>
Self-hosted services behind non-static IP address AKA your normal internet home connection. It periodically updates the route53 A record with my dynamic IP address only if it changes.

<h2>Environment variables:</h2>
<ul>
<li> AWS_REGION <i>{see AWS regions table below (e.g. us-east-1)}</i></li>
<li>HOSTING_ZONE_ID <i>{get this from your AWS Route53 console management GUI}</i></li>

<li>DOMAIN_NAME <i>{the domain who's A record you would like updated automatically)</i></li>

<li>ACCESS_KEY_ID <i>{AWS account access key with read/write access to Route53}</i></li>

<li>SECRET_KEY <i>{secret key of the AWS account-obtain this when you created the user on AWS}</i></li>

<li>SYNC_EVERY_SECONDS <I>{Check for mismatch of external IP address interval in seconds}</i></li>
<li>RECORD_IDS <I>{Only update A record(s) listed here. You can have 1 or multiple seperated by comma eg. RECORD_IDS= ID1  eg.2 RECORD_IDS= ID1,ID2,ID3 }</i></li>
</ul>

<br/>
<b>AWS regions</b> (use value for HOSTING_ZONE_ID environment variable according to your region) 
<table >
<tr>
     <th>Region</th>
     <th>Value</th>
</tr>
	<tbody>
		<tr>
			<td>Asia Pacific (Hong Kong) </td>
			<td>ap-east-1</td>
		</tr>
		<tr>
			<td>Asia Pacific (Tokyo)  </td>
			<td>ap-northeast-1</td>
		</tr>
		<tr>
			<td>Asia Pacific (Seoul) </td>
			<td>ap-northeast-2</td>
		</tr>
		<tr>
			<td>Asia Pacific (Mumbai) </td>
			<td>ap-south-1</td>
		</tr>
		<tr>
			<td>Asia Pacific (Singapore) </td>
			<td>ap-southeast-1</td>
		</tr>
		<tr>
			<td>Asia Pacific (Sydney) </td>
			<td>ap-southeast-2</td>
		</tr>
		<tr>
			<td>Canada (Central)</td>
			<td>ca-central-1</td>
		</tr>
		<tr>
			<td>EU Central (Frankfurt)</td>
			<td>eu-central-1</td>
		</tr>
		<tr>
			<td>EU North (Stockholm) </td>
			<td>eu-north-1</td>
		</tr>
		<tr>
			<td>EU West (Ireland)  </td>
			<td>eu-west-1</td>
		</tr>
		<tr>
			<td>EU West (London) </td>
			<td>eu-west-2</td>
		</tr>
		<tr>
			<td>EU West (Paris)</td>
			<td>eu-west-3</td>
		</tr>
		<tr>
			<td>South America (Sao Paulo)</td>
			<td>sa-east-1</td>
		</tr>
		<tr>
			<td>US East (Virginia) </td>
			<td> us-east-1</td>
		</tr>
		<tr>
			<td>{US East (Ohio)</td>
			<td>us-east-2</td>
		</tr>
		<tr>
			<td>US West (N. California)</td>
			<td>us-west-1</td>
		</tr>
		<tr>
			<td>US West (Oregon)</td>
			<td>us-west-2</td>
		</tr>
		<tr>
			<td>China (Beijing)</td>
			<td>cn-north-1</td>
		</tr>
		<tr>
			<td>China (Ningxia)</td>
			<td>cn-northwest-1</td>
		</tr>
		<tr>
			<td>US GovCloud East (Virginia) </td>
			<td>us-gov-east-1</td>
		</tr>
		<tr>
			<td>US GovCloud West (Oregon)</td>
			<td>us-gov-west-1</td>
		</tr>		
	</tbody>
</table>
