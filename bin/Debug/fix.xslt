<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions">
	
	<xsl:template match="*|@*|text()">
		<xsl:copy>
			<xsl:apply-templates select="*|@*|text()"/>
		</xsl:copy>
	</xsl:template>
	
	<xsl:template match="StartTime|EndTime" priority="2">
		<xsl:copy>
			<xsl:value-of select="substring(.,1,19)"/>
		</xsl:copy>
	</xsl:template>
	
</xsl:stylesheet>
